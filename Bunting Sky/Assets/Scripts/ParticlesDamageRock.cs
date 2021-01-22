using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesDamageRock : MonoBehaviour
{
    public ParticleSystem partSysShurikenDamage;

    private ParticleSystem.EmitParams partSysShurikenDamageEmitParameters;
    [System.NonSerialized] public int partSysShurikenDamageEmitCount = 0;
    [System.NonSerialized] public float partSysShurikenDamageShapeRadius = 0.1f;
    [System.NonSerialized] public float partSysShurikenDamageSizeMultiplier = 1f;

    public float saturationDefault = 0.78f;

    public void SetParticleSystemDamageColour(Transform model, float saturationMultiplier)
    {
        //Assign type color to damage particle material
        Color activeModelMaterialColor = model.GetComponent<MeshRenderer>().material.GetColor("_Tint");
        Color materialColorRGB = new Color(
            activeModelMaterialColor.r,
            activeModelMaterialColor.g,
            activeModelMaterialColor.b,
            1f
        );

        //REDUCE SATURATION
        //Convert to HSV colour space
        Color.RGBToHSV(
            materialColorRGB,
            out float materialColorRGB_H,
            out float materialColorRGB_S,
            out float materialColorRGB_V
        );
        //Modify saturation
        materialColorRGB_S *= saturationMultiplier;
        //Convert back to RGB colour space
        materialColorRGB = Color.HSVToRGB(
            materialColorRGB_H,
            materialColorRGB_S,
            materialColorRGB_V
        );

        partSysShurikenDamageEmitParameters = new ParticleSystem.EmitParams
        {
            startColor = materialColorRGB
        };
    }

    public void EmitDamageParticles(int countMultiplier, Vector3 directionIn, Vector3 positionIn, bool destroyingEntireAsteroid)
    {
        /*
         * If destroyingEntireAsteroid flag is true:
         *  - emits particles in a sphere
         *  - with larger particles
         *  - with more particles
         *  - at a starting shape radius equal to the asteroid model radius
         *  - all from the centre of the asteroid, ignoring the specified position
         *  
         * Otherwise
         *  - emits 90% of particles in a cone shape
         *  - with the last 10% in a sphere shape
         *  - all from the specified position
         */

        //Shape radius/position, and size multiplier
        Vector3 directionOut;
        float sizeMultiplier = 1f;


        if (destroyingEntireAsteroid)
        {
            //Shape radius = model radius
            ParticleSystem.ShapeModule partSysShurikenDamageShapeModule = partSysShurikenDamage.shape;

            //For some reason this method just doesn't seem to return with the correct radius
            //Vector3 modelSize = activeModel.transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size;
            //smallestRadius = 0.5f * Mathf.Min(Mathf.Min(modelSize.x, modelSize.y), modelSize.z);
            //float averageRadius = (modelSize.x + modelSize.y + modelSize.z) / 6f; //divide by (n terms * 2) to get radius instead of diameter

            partSysShurikenDamageShapeModule.radius = partSysShurikenDamageShapeRadius;

            //Position and size
            //^^particleSystemDamageEmitParameters.position = transform.position;
            partSysShurikenDamageEmitParameters.position = Vector3.zero;
            partSysShurikenDamageEmitParameters.applyShapeToPosition = true;
            sizeMultiplier *= partSysShurikenDamageSizeMultiplier;
        }
        else
        {
            //Position
            partSysShurikenDamageEmitParameters.applyShapeToPosition = false;
            //^^particleSystemDamageEmitParameters.position = positionIn;
            partSysShurikenDamageEmitParameters.position = positionIn - transform.position;
        }

        //Velocity/rotation
        //particleSystemDamageEmitParameters.angularVelocity = 0f;
        //particleSystemDamageEmitParameters.rotation = 0f;

        //Per particle:
        float loops = partSysShurikenDamageEmitCount * countMultiplier;
        for (int i = 0; i <= loops; i++)
        {

            //Direction
            float directionCurve = Random.Range(0f, 3f);
            if (destroyingEntireAsteroid)
            {
                //Spherical because destroying entire asteroid
                directionOut = Random.insideUnitSphere.normalized;
            }
            else if (i >= loops - (loops * 0.1f))
            {
                //Cone has last 10% spherical
                directionOut = Random.insideUnitSphere.normalized;
            }
            else
            {
                //Cone
                float coneRadius = Random.Range(3f, 10f);
                directionOut = directionIn
                    + (Vector3.forward * Random.value * coneRadius)
                    + (Vector3.up * Random.value * coneRadius)
                    + (Vector3.right * Random.value * coneRadius);
            }

            //Velocity
            //^^particleSystemDamageEmitParameters.velocity = rb.velocity + (directionOut * directionCurve);
            partSysShurikenDamageEmitParameters.velocity = directionOut * directionCurve;

            //Size
            partSysShurikenDamageEmitParameters.startSize = Random.Range(0.03f * sizeMultiplier, 0.15f * sizeMultiplier);

            //Emit
            partSysShurikenDamage.Emit(
                partSysShurikenDamageEmitParameters,
                1
            );
        }
    }
}
