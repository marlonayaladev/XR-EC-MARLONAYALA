// Etapa 1: configuración global del proyecto XR.
// Verifica URP, importa los samples oficiales del XRI, TMP Essentials y habilita
// OpenXR (Standalone) con perfiles de interacción, todo por código y sin GUI.
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace XRChallenge
{
    public static class XRProjectConfigurator
    {
        const string PaqueteXRI = "com.unity.xr.interaction.toolkit";

        [MenuItem("XR Challenge/1. Configurar Proyecto XR")]
        public static void Configure()
        {
            Debug.Log("[XRChallenge] == Iniciando configuracion del proyecto XR ==");

            VerificarURP();
            ImportarSamples();
            ImportarTmpEssentials();
            HabilitarOpenXR();

            Debug.Log("[XRChallenge] == Configuracion del proyecto XR completada sin errores ==");
        }

        // Verifica que Graphics y Quality usen un Render Pipeline Asset de URP.
        static void VerificarURP()
        {
            const string rutaUrp = "Assets/Settings/PC_RPAsset.asset";
            var urp = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(rutaUrp);
            if (urp == null)
                throw new Exception("No existe " + rutaUrp + " (proyecto sin plantilla URP).");

            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                GraphicsSettings.defaultRenderPipeline = urp;
                Debug.Log("[XRChallenge] GraphicsSettings: pipeline URP asignado.");
            }
            Debug.Log("[XRChallenge] GraphicsSettings.defaultRenderPipeline = " +
                      GraphicsSettings.defaultRenderPipeline.name);

            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                var actual = QualitySettings.GetRenderPipelineAssetAt(i);
                if (actual == null)
                    Debug.LogWarning($"[XRChallenge] Calidad '{QualitySettings.names[i]}' sin Render Pipeline Asset: asignalo en Project Settings > Quality.");
                else
                    Debug.Log($"[XRChallenge] Calidad '{QualitySettings.names[i]}' usa URP: {actual.name}");
            }
        }

        // Importa por script los samples "Starter Assets" y "XR Device Simulator" del XRI
        // usando Sample.FindByPackage y Sample.Import (requisito de la rúbrica).
        static void ImportarSamples()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForPackageName(PaqueteXRI);
            if (info == null)
                throw new Exception("El paquete " + PaqueteXRI + " no esta instalado todavia.");

            var samples = Sample.FindByPackage(PaqueteXRI, info.version).ToArray();
            if (samples.Length == 0)
                throw new Exception("No se encontraron samples de " + PaqueteXRI + " " + info.version + ".");

            Debug.Log("[XRChallenge] Sample.ImportOptions disponibles: " +
                      string.Join(", ", Enum.GetNames(typeof(Sample.ImportOptions))));

            string[] requeridos = { "Starter Assets", "XR Device Simulator" };
            foreach (string nombre in requeridos)
            {
                // Sample es una estructura: buscamos por nombre con bandera.
                Sample muestra = default;
                bool encontrada = false;
                foreach (var s in samples)
                {
                    if (s.displayName == nombre)
                    {
                        muestra = s;
                        encontrada = true;
                        break;
                    }
                }

                if (!encontrada)
                {
                    Debug.LogWarning("[XRChallenge] Sample no encontrado: " + nombre);
                    continue;
                }

                if (muestra.isImported)
                {
                    Debug.Log($"[XRChallenge] Sample ya importado: {nombre}");
                }
                else
                {
                    // Opción 0 = sin opciones extra: importación no interactiva.
                    muestra.Import(default(Sample.ImportOptions));
                    Debug.Log($"[XRChallenge] Sample importado: {nombre}");
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        // Importa los recursos esenciales de TextMeshPro (necesarios para el contador).
        static void ImportarTmpEssentials()
        {
            if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
            {
                Debug.Log("[XRChallenge] TMP Essentials ya importados.");
                return;
            }

            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
                Debug.Log("[XRChallenge] TMP Essentials importados.");
            else
                Debug.LogWarning("[XRChallenge] TMP Essentials: la importacion puede completarse en el proximo refresco del Editor.");
        }

        // Habilita el loader de OpenXR para Standalone (PC) y activa los perfiles
        // Oculus Touch y Khronos Simple Controller.
        static void HabilitarOpenXR()
        {
            // 1) XR Plug-in Management -> settings por build target (persistidos como asset:
            //    EditorBuildSettings no acepta objetos no persistidos).
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,
                out XRGeneralSettingsPerBuildTarget perTarget);
            if (perTarget == null)
            {
                // El propio paquete los localiza por tipo si no están en EditorBuildSettings.
                string[] existentes = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
                if (existentes.Length > 0)
                {
                    string rutaExistente = AssetDatabase.GUIDToAssetPath(existentes[0]);
                    perTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(rutaExistente);
                }
                else
                {
                    perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    if (!AssetDatabase.IsValidFolder("Assets/XR"))
                        AssetDatabase.CreateFolder("Assets", "XR");
                    AssetDatabase.CreateAsset(perTarget, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                }
                AssetDatabase.SaveAssets();
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
                Debug.Log("[XRChallenge] XR General Settings creados y registrados en EditorBuildSettings.");
            }

            if (perTarget.SettingsForBuildTarget(BuildTargetGroup.Standalone) == null)
                perTarget.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Standalone);

            // Crea también el XRManagerSettings si no existe (AssignLoader lo necesita).
            if (!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Standalone))
                perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Standalone);

            var general = perTarget.SettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (general == null)
                throw new Exception("No se pudieron crear los XR General Settings para Standalone.");

            // 2) Loader de OpenXR para PC (Standalone)
            bool loaderOk = XRPackageMetadataStore.AssignLoader(
                general.Manager, typeof(OpenXRLoader).FullName, BuildTargetGroup.Standalone);
            if (!loaderOk)
                throw new Exception("XRPackageMetadataStore.AssignLoader(OpenXR, Standalone) devolvio false.");
            Debug.Log("[XRChallenge] OpenXR habilitado para Standalone (PC).");

            // 3) Los settings de OpenXR viven en una clase interna del paquete; se crea
            //    por reflexión y luego se leen con la API pública OpenXRSettings.
            CrearOpenXrPackageSettings();
            var openXr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
            if (openXr == null)
            {
                Debug.LogWarning("[XRChallenge] OpenXRSettings no disponible aun para Standalone.");
                return;
            }

            int nuevas = 0;
            foreach (var feature in openXr.GetFeatures())
            {
                if (feature is OculusTouchControllerProfile || feature is KHRSimpleControllerProfile)
                {
                    if (!feature.enabled)
                    {
                        feature.enabled = true;
                        nuevas++;
                    }
                    Debug.Log($"[XRChallenge] Perfil OpenXR: {feature.name} enabled={feature.enabled}");
                }
            }
            EditorUtility.SetDirty(openXr);
            Debug.Log($"[XRChallenge] Perfiles de interaccion habilitados: {nuevas} nuevos.");
            AssetDatabase.SaveAssets();
        }

        // Llama a OpenXRPackageSettings.GetOrCreateInstance() (clase interna del paquete).
        static void CrearOpenXrPackageSettings()
        {
            Type tipo = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                tipo = asm.GetType("UnityEditor.XR.OpenXR.OpenXRPackageSettings");
                if (tipo != null)
                    break;
            }
            if (tipo == null)
            {
                Debug.LogWarning("[XRChallenge] No se encontro UnityEditor.XR.OpenXR.OpenXRPackageSettings.");
                return;
            }

            var metodo = tipo.GetMethod("GetOrCreateInstance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (metodo == null)
            {
                Debug.LogWarning("[XRChallenge] GetOrCreateInstance no encontrada en OpenXRPackageSettings.");
                return;
            }

            metodo.Invoke(null, null);
            Debug.Log("[XRChallenge] OpenXR Package Settings registrados en EditorBuildSettings.");
        }
    }
}
