void MainLightFlipped_half(float3 WorldPos, out half3 Direction, out half3 Color, out half Attenuation)
{
#if SHADERGRAPH_PREVIEW
	Direction = half3(0.5, 0.5, 0);
	Color = 1;
	Attenuation = 1;
#else
#if SHADOWS_SCREEN
	half4 clipPos = TransformWorldToHClip(WorldPos);
	half4 shadowCoord = ComputeScreenPos(clipPos);
#else
	half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
	Light mainLightFlipped = GetMainLight(shadowCoord);
	Direction = mainLightFlipped.direction;
	Color = mainLightFlipped.color;
	Attenuation = mainLightFlipped.distanceAttenuation * mainLightFlipped.shadowAttenuation;
#endif
}
