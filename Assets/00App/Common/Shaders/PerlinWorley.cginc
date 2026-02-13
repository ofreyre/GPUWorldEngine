#ifndef dbg_perlinworleynoise
  #define dbg_perlinworleynoise

#ifndef dbg_perlinnoise
  #include "PerlinNoise.cginc"
#endif

#ifndef dbg_worleynoise
  #include "CellularNoise.cginc"
#endif

float PerlinWorley3D(float3 p, 
                    float perlinWeight, float perlinScale, float perlinCellLength, float perlinPeriod, uint perlinLayers, float perlinPersistance, float perlinRoughness,
                                        float voronoiScale, float voronoiCellLength, float voronoiPeriod, uint voronoiLayers, float voronoiPersistance, float voronoiRoughness
)
{
    float perlin = PerlinNoise3Tiledlayered(p, perlinScale, perlinCellLength, perlinPeriod, perlinLayers, perlinPersistance, perlinRoughness);
    float cellular = Cellular3dtiledlayered(p, voronoiScale, voronoiCellLength, voronoiPeriod, voronoiLayers, voronoiPersistance, voronoiRoughness);
    return perlin * perlinWeight + (1 - cellular) * (1 - perlinWeight);
}


#endif




