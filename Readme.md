## GPU World Engine

Unity 2021.3.4f1

Platforms Windows and Mac

Render Pipeline: URP

Demo Path: Assets/00App/Terrain/Scenes/Terrain.unity

Procedural world 1 - infinite terrain, biomes, boids, mesh colliders, raycasting

[![Procedural terrain](https://img.youtube.com/vi/9bUlBSgPLdE/0.jpg)](https://www.youtube.com/watch?v=9bUlBSgPLdE)

Procedural world 2 - anti tiling and tiles blend

[![Anti tiling and tiles blending](https://img.youtube.com/vi/c8dHoBwyLYU/0.jpg)](https://www.youtube.com/watch?v=c8dHoBwyLYU)

Procedural world 3 - volumetric fog

[![Volumetric Fog](https://img.youtube.com/vi/WhBsMclZxTg/0.jpg)](https://www.youtube.com/watch?v=WhBsMclZxTg)

Procedural world 4 - Animation clips with shader

[![Shader mesh animation](https://img.youtube.com/vi/BdypoZteFrE/0.jpg)](https://www.youtube.com/watch?v=BdypoZteFrE)

Procedural world 5 - Animation clips with shader in the gameplay

[![Shader mesh animation](https://img.youtube.com/vi/xqzZoInsfEo/0.jpg)](https://www.youtube.com/watch?v=xqzZoInsfEo)

### Architecture

Because this is my first major experiment with compute shaders, the architecture has been modified and optimized as I better understand the compute shader pipeline.
For performance, I will try to parallelize the CPU tasks as much as possible as well.

The world data is organized by chunk, and each chunk is responsible for maintaining the mesh, collision and boids data in its 3D space.
For efficiency, the world is also divided in a grid spatial partition.

The code is structured by a main manager (TerrainManager) which organize and distribute the work to its components:

1. ChunksManager: handle the chunks, destroying, creating and saving the chunks data according to the camera position.
2. BoidsManager: maintains a list of active chunks according to the camera position and coordinates the data to and from its computer.
3. RaysManager: solves collisions between the shots of the player and the active boids.

The BoidsManager and RaysManager delegate the handle of the compute shaders to their computers (BoidsComputer and RaysCollisionComputer)
Each chunk in ChunkManager has four computers that handle their compute shaders to create the chunk content in the following order:

1. HeightMapComputer: uses a cyclic 2D perlin noise with a cubic hermite spline to have more control over the height.
2. BiomesComputer: uses the HeightMapComputer compute buffers to produce a texture used in the Terrain shader.
3. MeshComputer: uses the HeightMapComputer compute buffers to generate the vertices, triangles, normals and collision data of the terrain chunk.
3. BoidsInitComputer: uses the HeightMapComputer compute buffers to generate the data to create the boids of the chunk.

#### The boids compute shaders

These are the most interesting shaders. The calculation of the boids behavior are divided into three steps in the following order:

1. Bitonic sort: order the boids by the grid cell corresponding to their position in an array A.
2. Hashed grid: fills an array with the index in A of the first boid in the grid cell.
3. Boids behavior: applies forces to each boid, to its target and from boids in their neighborhood, and uses the mesh collider data to set the Y coordinate.
 
#### The raycast compute shader

Discretize each ray in grid cells and check against the boids in each ray cell and its neighborhood.

### Terrain shader: anti tiling and blending

I rotated and scaled each tile to reduce tiling and blend it with the its neighbor tiles using cell noise and gaussian function for the weights.
Then apply a triplanar projection to the resulting coordinates and transform them to uvs for the albedo and normals textures.

### Volumetric Fog

#### Producing a tiled noise

The code is in the path: \Assets\00App\Fog

The compute shader:

1. Blend Cellular and Perlin 3D tiled noises.
2. Pack the noises settings data to calculate in the same loops both Cellular and Perlin.
3. Chose to step instead of if to calculate all instead of branch.
4. Structure the code to use it in the preview and in the bake without losing performance.

#### Baking the tiled noise into a Texture3D asset

1. Edit two detail levels.
2. Edit each channel separately.
3. Edit each noise (Cellular and Perlin) separately.
4. Preview of slice Y. Can change value of Y.
5. Calculate the noise in with the compute shader and use the resulting StructuredBuffer to fill the Texture3D.
6. Save noise settings in a ScriptableObject.

The Noise Editor

![Main State Machine](https://drive.google.com/uc?export=view&id=1tjImnitzPd1TOrxMNsv_haLoVHegYUUb "Editor")

The Baked Noise in a Texture3D

![Main State Machine](https://drive.google.com/uc?export=view&id=1KCFn4CLIIUsoC_awFQhPf6KU7oMx5wuy "Editor")

#### The fog shader

Raymarching shader that samples the backed noise 3D texture to calculate the fog density, and the camera depth texture to calculate the number of steps and blend the fog with the scene meshes.

The interesting part of the shader was to avoid the artifacts of the meshes near the camera. To achieve that, I increase the size of the raymarching steps exponentially:

S(i) = exp(a * i - b)

So

sampleDistance = exp(a * i - b);

Where i is the step, a natural nulber in [0, NUMBER OF STEPS],  b determines the value of S(0), this is, the size of the first step, and a the derivative of S.

NUMBER OF STEPS and b are chosen, so a is solved by:

a = (log(totalDistance) + b) / NUMBER OF STEPS

The graph of S is

![Step size function](https://drive.google.com/uc?export=view&id=1Thx2iCOaue-o0uX-TibCu4hhfSmn8i4A "Step size function")

The density function is

D(step) = exp(3 * step - 5)

So

density *= exp(3 * step - 5);

Where step is normalized by i / NUMBER OF STEPS. The ful calculations of the fog fragment are:

1. The noise is sapled, its fours channels are blended by a dot product with their wights: 

float4 noise = SAMPLE_TEXTURE3D(_BaseMap, sampler_BaseMap, p);

float density = dot(noise, _Weights);

2. The density is wighted by the function D:

density *= exp(3 * step - 5);

3. The density is added to the fog:

fog += density;

The graph of D is

![Density function](https://drive.google.com/uc?export=view&id=19fNNbc0RLChIU3KVF1bhPeqEkuSjqx2U "Fog Density function")

This is the resulting volumetric of the fog

[![Volumetric Fog](https://img.youtube.com/vi/WhBsMclZxTg/0.jpg)](https://www.youtube.com/watch?v=WhBsMclZxTg)

#### Working On

Volumetric fog: lighting, shadows, sun, god rays.

### Animation clips with shader

Produce data necessary to animate clips with a custom shader, make the custom shader, and integrate all to the game.

#### Animation data and shader

The system supports:
- Bones animations
- Multiple meshes
- Multiple clips
- One material per mesh

This was done by:
- Remapping the bones index of each mesh of the model
- Baking all bones each frame
- Remapping the binds between the vertices and the bones.

The shader is made with Shader Graph and the animation logic is in a Custom Node. The animation data is passed to the shader by compute buffers.

The editable data to bake a model with its clips is the class AnimatedModelData: ScriptableObject:

![Editable animated model data](https://drive.google.com/uc?export=view&id=1VN_WThUzlY6ReTaiNv2-OiVP5jdhmiAt "Editable animated model data")

It has its own custom editor:

![Custom editor](https://drive.google.com/uc?export=view&id=1bm1orW0q3Q2ENMYsbBR0jUtfWzh8Bx0Y "Custom editor")

The data produced after baking to assets from the custom editor is:

- An instance of the class ModelAnimatorController: ScriptableObject.
- The meshes of the model.
- One material per mesh.
- One prefab with the hierarchy of all the meshes.

![Baked assets](https://drive.google.com/uc?export=view&id=1cF6JVJCAhxpHnaNg1LtIWkPYL8OoRcwN "Baked assets")

The instance of the ModelAnimatorController contains all the data that the shader needs to play each clip.
- An array of bones weights per vertex start indices. The index of this array correspond to the vertexID
- An array of { weight, boneID } mapped by previous array.
- Bones count.
- An array of clips frames start index.
- A Matrix4x4 array of the transforms of each bone per frame for all clips mapped by previous array.

![ModelAnimatorController](https://drive.google.com/uc?export=view&id=1nI0FMSxEZcLA1iuXAuS-H6zcN239JMj9 "ModelAnimatorController")

As result, the prefabs are animated by the shader and a ModelAnimator component, with no bones game objects and no Animator component.

[![Shader mesh animation](https://img.youtube.com/vi/BdypoZteFrE/0.jpg)](https://www.youtube.com/watch?v=BdypoZteFrE)

#### Integration to game

The only new functionality necessary to integrate the shader animation to the scene, was to initialize and release the necessary compute buffers of the meterials of all the prefabs.

The ModelAnimator component of each instance calls ModelAnimatorController.SetClip to initialize the instance data of each prefab with a property block.

This is the result of integrating the baked prefabs and the animation shader to the gameplay.

[![Shader mesh animation](https://img.youtube.com/vi/xqzZoInsfEo/0.jpg)](https://www.youtube.com/watch?v=xqzZoInsfEo)


### To Do

1. Gravity. DONE
2. Rivers based on climate and biomes.
3. Vegetation based on water, climate and biomes. 	ON PROGRESS
4. Collision with static objects. DONE
6. Animation baking. DONE
7. Mesh animation with compute shader. DONE
8. FSM with compute shader.
9. Migrate to DOTS