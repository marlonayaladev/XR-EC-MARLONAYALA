// Etapa 2 y 3: construcción POR CÓDIGO de la escena XR (nada de YAML manual).
// Menú: XR Challenge/2. Construir Escena
// Batch: -executeMethod XRChallenge.XRSceneBuilder.Build
// Es idempotente: cada ejecución reconstruye la escena desde cero (NewScene),
// reutiliza los materiales existentes y no duplica la escena en Build Settings.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Management;
using TMPro;

namespace XRChallenge
{
    public static class XRSceneBuilder
    {
        public const string NombreEscena = "EC_XR_MARLONAYALA";
        public static readonly string RutaEscena = "Assets/Scenes/" + NombreEscena + ".unity";

        // Registro compartido de criterios (lo consume Verify()).
        static readonly List<string> _fallos = new List<string>();

        static void Check(bool ok, string criterio)
        {
            Debug.Log($"[CHECK][{(ok ? "PASS" : "FAIL")}] {criterio}");
            if (!ok)
                _fallos.Add(criterio);
        }

        // Referencias que usan las etapas posteriores (rayo, contador, teletransporte).
        public static Light LuzPuntual { get; private set; }
        public static GameObject Piso { get; private set; }

        [MenuItem("XR Challenge/2. Construir Escena")]
        public static void Build()
        {
            Debug.Log("[XRChallenge] Construyendo escena: " + RutaEscena);

            if (!Application.isBatchMode)
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CrearIluminacion();
            Piso = CrearPiso();
            CrearParedes();
            CrearMesa();
            CrearObjetosEscenario();
            CrearObjetosManipulables();
            InstanciarRig();
            CrearInteractionManager();
            CrearEventSystem();
            CrearBotonesInteraccion();
            ConfigurarTeletransporte();
            CrearContador();
            CrearSimuladorDispositivo();

            GuardarEscena(escena);
        }

        // ---------------------------------------------------------------- escenario

        // Direccional + Point Light interactiva (la usará el Boton_Luz con el rayo).
        static void CrearIluminacion()
        {
            var dirGo = new GameObject("Directional Light");
            var dir = dirGo.AddComponent<Light>();
            dir.type = LightType.Directional;
            dir.intensity = 1.1f;
            dir.color = Color.white;
            dir.shadows = LightShadows.Soft;
            dirGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var puntoGo = new GameObject("LuzInteractiva");
            puntoGo.transform.position = new Vector3(0f, 2.7f, 1.2f);
            LuzPuntual = puntoGo.AddComponent<Light>();
            LuzPuntual.type = LightType.Point;
            LuzPuntual.range = 9f;
            LuzPuntual.intensity = 2.2f;
            LuzPuntual.color = new Color(1f, 0.92f, 0.75f);
            LuzPuntual.shadows = LightShadows.Soft;
            Debug.Log("[XRChallenge] Iluminacion creada (Directional + Point 'LuzInteractiva').");
        }

        // Piso con BoxCollider (también sera el Teleportation Area en la Etapa 5).
        static GameObject CrearPiso()
        {
            var piso = Primitiva(PrimitiveType.Cube, "Piso", null,
                new Vector3(0f, -0.1f, 0f), new Vector3(12f, 0.2f, 12f));
            piso.GetComponent<MeshRenderer>().sharedMaterial =
                ObtenerMaterial("M_Piso", new Color(0.55f, 0.58f, 0.62f));
            Debug.Log("[XRChallenge] Piso con Collider creado (12 x 12 m).");
            return piso;
        }

        // 4 barreras perimetrales con material propio.
        static void CrearParedes()
        {
            var mat = ObtenerMaterial("M_Muro", new Color(0.82f, 0.86f, 0.9f));
            var datos = new[]
            {
                new { nombre = "Muro_Norte", pos = new Vector3(0f, 1.5f, 6f),   esc = new Vector3(12.2f, 3f, 0.2f) },
                new { nombre = "Muro_Sur",   pos = new Vector3(0f, 1.5f, -6f),  esc = new Vector3(12.2f, 3f, 0.2f) },
                new { nombre = "Muro_Este",  pos = new Vector3(6f, 1.5f, 0f),   esc = new Vector3(0.2f, 3f, 12.2f) },
                new { nombre = "Muro_Oeste", pos = new Vector3(-6f, 1.5f, 0f),  esc = new Vector3(0.2f, 3f, 12.2f) },
            };
            foreach (var d in datos)
            {
                var muro = Primitiva(PrimitiveType.Cube, d.nombre, null, d.pos, d.esc);
                muro.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
            Debug.Log("[XRChallenge] 4 muros perimetrales creados.");
        }

        // Mesa hecha de cubos (tablero + 4 patas), con Colliders.
        static void CrearMesa()
        {
            var mesa = new GameObject("Mesa");
            var mat = ObtenerMaterial("M_Mesa", new Color(0.45f, 0.3f, 0.18f));

            var tablero = Primitiva(PrimitiveType.Cube, "Mesa_Tablero", mesa.transform,
                new Vector3(0f, 0.9f, 1.5f), new Vector3(2.2f, 0.1f, 1.2f));
            tablero.GetComponent<MeshRenderer>().sharedMaterial = mat;

            foreach (float x in new[] { -1f, 1f })
                foreach (float z in new[] { -0.5f, 0.5f })
                {
                    var pata = Primitiva(PrimitiveType.Cube, "Mesa_Pata", mesa.transform,
                        new Vector3(x, 0.425f, 1.5f + z), new Vector3(0.1f, 0.85f, 0.1f));
                    pata.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            Debug.Log("[XRChallenge] Mesa de cubos creada (tablero a 0.95 m con Collider).");
        }

        // Cilindro y cápsula decorativos (cubren los 5+ objetos 3D de la rúbrica).
        static void CrearObjetosEscenario()
        {
            var cilindro = Primitiva(PrimitiveType.Cylinder, "Cilindro", null,
                new Vector3(2.6f, 0.5f, 2.6f), new Vector3(0.5f, 0.5f, 0.5f));
            cilindro.GetComponent<MeshRenderer>().sharedMaterial =
                ObtenerMaterial("M_Cilindro", new Color(0.15f, 0.65f, 0.35f));

            var capsula = Primitiva(PrimitiveType.Capsule, "Capsula", null,
                new Vector3(-2.6f, 0.5f, 2.6f), new Vector3(0.5f, 0.5f, 0.5f));
            capsula.GetComponent<MeshRenderer>().sharedMaterial =
                ObtenerMaterial("M_Capsula", new Color(0.55f, 0.25f, 0.8f));
            Debug.Log("[XRChallenge] Objetos decorativos creados: Cilindro, Capsula.");
        }

        // 3 objetos manipulables sobre la mesa: cubo, esfera y llave.
        static void CrearObjetosManipulables()
        {
            // 1) Cubo naranja
            var cubo = Primitiva(PrimitiveType.Cube, "CuboAgarre", null,
                new Vector3(-0.7f, 1.1f, 1.5f), Vector3.one * 0.3f);
            cubo.GetComponent<MeshRenderer>().sharedMaterial =
                ObtenerMaterial("M_Cubo", new Color(0.95f, 0.5f, 0.1f));
            HacerManipulable(cubo);

            // 2) Esfera azul
            var esfera = Primitiva(PrimitiveType.Sphere, "EsferaAgarre", null,
                new Vector3(0f, 1.1f, 1.5f), Vector3.one * 0.3f);
            esfera.GetComponent<MeshRenderer>().sharedMaterial =
                ObtenerMaterial("M_Esfera", new Color(0.15f, 0.4f, 0.9f));
            HacerManipulable(esfera);

            // 3) Llave dorada construida con primitivas (raíz con BoxCollider propio).
            var llave = new GameObject("LlaveAgarre");
            llave.transform.position = new Vector3(0.7f, 1.02f, 1.5f);
            llave.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // apoyada en la mesa
            var collider = llave.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = new Vector3(0.14f, 0.78f, 0.14f);

            var matLlave = ObtenerMaterial("M_Llave", new Color(0.95f, 0.8f, 0.2f));
            var eje = Primitiva(PrimitiveType.Cylinder, "Llave_Eje", llave.transform,
                Vector3.zero, new Vector3(0.06f, 0.17f, 0.06f));
            eje.GetComponent<MeshRenderer>().sharedMaterial = matLlave;
            var cabeza = Primitiva(PrimitiveType.Cube, "Llave_Cabeza", llave.transform,
                new Vector3(0f, 0.28f, 0f), new Vector3(0.18f, 0.18f, 0.06f));
            cabeza.GetComponent<MeshRenderer>().sharedMaterial = matLlave;
            var diente1 = Primitiva(PrimitiveType.Cube, "Llave_Diente1", llave.transform,
                new Vector3(0.07f, -0.22f, 0f), new Vector3(0.08f, 0.08f, 0.06f));
            diente1.GetComponent<MeshRenderer>().sharedMaterial = matLlave;
            var diente2 = Primitiva(PrimitiveType.Cube, "Llave_Diente2", llave.transform,
                new Vector3(0.07f, -0.32f, 0f), new Vector3(0.08f, 0.1f, 0.06f));
            diente2.GetComponent<MeshRenderer>().sharedMaterial = matLlave;
            HacerManipulable(llave);

            Debug.Log("[XRChallenge] 3 objetos manipulables creados: CuboAgarre, EsferaAgarre, LlaveAgarre.");
        }

        // Rigidbody con gravedad + XR Grab Interactable (movement razonable y throw activo).
        static void HacerManipulable(GameObject go)
        {
            var rb = go.GetComponent<Rigidbody>();
            if (rb == null)
                rb = go.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var grab = go.GetComponent<XRGrabInteractable>();
            if (grab == null)
                grab = go.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            grab.throwOnDetach = true;
            grab.trackPosition = true;
            grab.trackRotation = true;
        }

        // ------------------------------------------------- Etapa 4: rayo (botones)

        // Dos botones con XR Simple Interactable que se activan apuntando con el
        // rayo (NearFarInteractor). Los eventos selectEntered se conectan por
        // código mediante los scripts ToggleLuz y CambiarColor (sin tocar Inspector).
        static void CrearBotonesInteraccion()
        {
            // Boton_Luz: enciende/apaga la Point Light 'LuzInteractiva'.
            var botonLuz = Primitiva(PrimitiveType.Cube, "Boton_Luz", null,
                new Vector3(-2.2f, 1.6f, 5.82f), new Vector3(0.35f, 0.35f, 0.15f));
            botonLuz.GetComponent<MeshRenderer>().sharedMaterial =
                ObtenerMaterial("M_BotonLuz", new Color(0.9f, 0.15f, 0.15f));
            botonLuz.AddComponent<XRSimpleInteractable>();
            var toggle = botonLuz.AddComponent<ToggleLuz>();
            toggle.luz = LuzPuntual; // conexión por código (requisito de la rúbrica)

            // Boton_Color: cambia el color de la Cápsula con MaterialPropertyBlock.
            var botonColor = Primitiva(PrimitiveType.Cube, "Boton_Color", null,
                new Vector3(2.2f, 1.6f, 5.82f), new Vector3(0.35f, 0.35f, 0.15f));
            botonColor.GetComponent<MeshRenderer>().sharedMaterial =
                ObtenerMaterial("M_BotonColor", new Color(0.1f, 0.7f, 0.9f));
            botonColor.AddComponent<XRSimpleInteractable>();
            var cambiar = botonColor.AddComponent<CambiarColor>();
            var capsula = GameObject.Find("Capsula");
            if (capsula != null)
                cambiar.objetivo = capsula.GetComponent<MeshRenderer>(); // conexión por código
            else
                Debug.LogWarning("[XRChallenge] No se encontró la Cápsula para Boton_Color.");

            Debug.Log("[XRChallenge] Boton_Luz y Boton_Color creados (XR Simple Interactable conectados por código).");
        }

        // ------------------------------------------- Etapa 5: reto libre (teleport)

        // Teleportation Area sobre el piso (capa 'Teleport' = bit 31, igual que el
        // prefab oficial de Starter Assets) + verificación del Teleportation Provider.
        static void ConfigurarTeletransporte()
        {
            var area = Piso.GetComponent<TeleportationArea>();
            if (area == null)
                area = Piso.AddComponent<TeleportationArea>();
            area.interactionLayers = 1 << 31;

            var proveedor = UnityEngine.Object.FindFirstObjectByType<TeleportationProvider>();
            if (proveedor == null)
            {
                var rig = GameObject.Find("XR Origin (XR Rig)");
                if (rig != null)
                    proveedor = rig.AddComponent<TeleportationProvider>();
            }

            Check(proveedor != null, "Teleportation Provider presente en la escena");
            Debug.Log("[XRChallenge] Teleportation Area configurado en el piso; Provider = " +
                      (proveedor != null ? proveedor.name : "NO ENCONTRADO"));
        }

        // Contador World Space con TextMeshPro: cuántas veces se encendió la luz.
        // El script ContadorLuces se conecta al ToggleLuz por código.
        static void CrearContador()
        {
            var canvasGo = new GameObject("ContadorLuces");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500f, 120f);
            canvasGo.transform.position = new Vector3(0f, 2.5f, 3.6f);
            canvasGo.transform.rotation = Quaternion.identity;
            canvasGo.transform.localScale = Vector3.one * 0.003f; // 1.5 x 0.36 m en el mundo

            var textoGo = new GameObject("TextoContador");
            textoGo.transform.SetParent(canvasGo.transform, false);
            var rectTexto = textoGo.AddComponent<RectTransform>();
            rectTexto.anchorMin = Vector2.zero;
            rectTexto.anchorMax = Vector2.one;
            rectTexto.offsetMin = Vector2.zero;
            rectTexto.offsetMax = Vector2.zero;
            var texto = textoGo.AddComponent<TextMeshProUGUI>();
            texto.text = "Veces encendida: 0";
            texto.fontSize = 72f;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = new Color(1f, 0.95f, 0.4f);

            var contador = canvasGo.AddComponent<ContadorLuces>();

            // Conexión por código: ToggleLuz -> ContadorLuces.
            var toggle = UnityEngine.Object.FindFirstObjectByType<ToggleLuz>();
            if (toggle != null)
                toggle.contador = contador;
            else
                Debug.LogWarning("[XRChallenge] No se encontró ToggleLuz para conectar el contador.");

            Debug.Log("[XRChallenge] Contador World Space (TMP) creado y conectado al Boton_Luz.");
        }

        // XR Device Simulator en la escena para probar sin visor.
        static void CrearSimuladorDispositivo()
        {
            string ruta = null;
            foreach (string guid in AssetDatabase.FindAssets("XR Device Simulator t:Prefab"))
            {
                string r = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(r) == "XR Device Simulator")
                {
                    ruta = r;
                    break;
                }
            }
            if (ruta == null)
            {
                Debug.LogWarning("[XRChallenge] Prefab 'XR Device Simulator' no encontrado (¿sample importado?).");
                return;
            }

            var simulador = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(ruta)) as GameObject;
            simulador.name = "XR Device Simulator";
            simulador.transform.position = Vector3.zero;
            Debug.Log("[XRChallenge] XR Device Simulator añadido a la escena.");
        }

        // ------------------------------------------------------------------- rig

        // Instancia el prefab oficial "XR Origin (XR Rig)" (Starter Assets).
        static void InstanciarRig()
        {
            string rutaRig = null;
            foreach (string guid in AssetDatabase.FindAssets("XR Origin t:Prefab"))
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(ruta) == "XR Origin (XR Rig)")
                {
                    rutaRig = ruta;
                    break;
                }
            }
            if (rutaRig == null)
                throw new Exception("Prefab 'XR Origin (XR Rig)' no encontrado. ¿Se importo el sample Starter Assets?");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rutaRig);
            var rig = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            rig.name = "XR Origin (XR Rig)";
            rig.transform.position = new Vector3(0f, 0f, -2.5f);
            rig.transform.rotation = Quaternion.identity;

            // La escena arranca vacía: la única cámara debe ser la del rig.
            var camaras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            if (camaras.Length != 1)
                Debug.LogWarning($"[XRChallenge] La escena tiene {camaras.Length} camaras (se esperaba solo la del XR Origin).");
            else
                Debug.Log("[XRChallenge] Camara unica del XR Origin verificada: " + camaras[0].name);
        }

        // Un solo XR Interaction Manager para toda la escena.
        static void CrearInteractionManager()
        {
            if (UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>() != null)
            {
                Debug.Log("[XRChallenge] Ya existia un XR Interaction Manager.");
                return;
            }
            new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            Debug.Log("[XRChallenge] XR Interaction Manager creado.");
        }

        // Un solo EventSystem, con el módulo de entrada XR de UI.
        static void CrearEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                Debug.Log("[XRChallenge] Ya existia un EventSystem.");
                return;
            }
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<XRUIInputModule>();
            Debug.Log("[XRChallenge] EventSystem + XRUIInputModule creado.");
        }

        // --------------------------------------------------------------- utilidades

        // Primitiva con collider; la posición es local (y mundial si no tiene padre).
        static GameObject Primitiva(PrimitiveType tipo, string nombre, Transform padre,
            Vector3 posicion, Vector3 escala)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nombre;
            if (padre != null)
                go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            go.transform.localScale = escala;
            return go;
        }

        // Material URP/Lit persistido en Assets/Materials (reutiliza si ya existe).
        static Material ObtenerMaterial(string nombre, Color color)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");

            string ruta = "Assets/Materials/" + nombre + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    throw new Exception("Shader 'Universal Render Pipeline/Lit' no encontrado (¿URP activo?).");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, ruta);
            }
            mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // Guarda la escena con el nombre exacto y la agrega a Build Settings.
        static void GuardarEscena(Scene escena)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            if (!EditorSceneManager.SaveScene(escena, RutaEscena))
                throw new Exception("No se pudo guardar la escena en " + RutaEscena);

            var escenas = EditorBuildSettings.scenes.ToList();
            if (!escenas.Any(s => s.path == RutaEscena))
            {
                escenas.Add(new EditorBuildSettingsScene(RutaEscena, true));
                EditorBuildSettings.scenes = escenas.ToArray();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[XRChallenge] Escena guardada y agregada a Build Settings: " + RutaEscena);
        }

        // ------------------------------------------------------- verificación

        // Recorre todos los criterios de aceptación de la rúbrica y lanza una
        // excepción (EXIT ≠ 0 en batch) si alguno falla.
        [MenuItem("XR Challenge/3. Verificar Escena")]
        public static void Verify()
        {
            _fallos.Clear();
            Debug.Log("[XRChallenge] == Verificacion de criterios de aceptacion ==");

            // En batch Unity no carga escenas automáticamente: la abrimos explícitamente.
            if (System.IO.File.Exists(RutaEscena))
                EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

            // Escena y build
            Scene abierta = SceneManager.GetActiveScene();
            Check(abierta.path == RutaEscena, "Escena abierta = " + abierta.path);
            Check(EditorBuildSettings.scenes.Any(s => s.path == RutaEscena && s.enabled),
                "Escena agregada y habilitada en Build Settings");

            // Configuración
            Check(GraphicsSettings.defaultRenderPipeline != null,
                "URP activo en GraphicsSettings = " +
                (GraphicsSettings.defaultRenderPipeline != null
                    ? GraphicsSettings.defaultRenderPipeline.name : "NINGUNO"));

            bool openXr = false;
            if (EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,
                    out XRGeneralSettingsPerBuildTarget perTarget) && perTarget != null)
            {
                var general = perTarget.SettingsForBuildTarget(BuildTargetGroup.Standalone);
                if (general != null && general.Manager != null)
                    openXr = general.Manager.activeLoaders.Any(
                        l => l != null && l.GetType().Name.Contains("OpenXR"));
            }
            Check(openXr, "OpenXR habilitado en XR Plug-in Management (Standalone)");

            // Escenario
            Check(GameObject.Find("Piso") != null, "Piso presente");
            Check(GameObject.Find("Piso") != null && GameObject.Find("Piso").GetComponent<Collider>() != null,
                "Piso con Collider");
            Check(GameObject.Find("Directional Light") != null, "Directional Light presente");
            int puntual = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
                .Count(l => l.type == LightType.Point);
            Check(puntual >= 1, "Point Light adicional presente (" + puntual + ")");
            int muros = new[] { "Muro_Norte", "Muro_Sur", "Muro_Este", "Muro_Oeste" }
                .Count(n => GameObject.Find(n) != null);
            Check(muros == 4, "Muros perimetrales: " + muros + "/4");
            int objetos = new[] { "CuboAgarre", "EsferaAgarre", "LlaveAgarre", "Cilindro", "Capsula", "Mesa" }
                .Count(n => GameObject.Find(n) != null);
            Check(objetos >= 5, "Objetos 3D distintos: " + objetos + " (minimo 5)");

            var materiales = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" })
                .Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(m => m != null).ToArray();
            bool shaderUrp = materiales.Length > 0 && materiales.All(m =>
                m.shader != null && m.shader.name.StartsWith("Universal Render Pipeline"));
            Check(shaderUrp, "Materiales en Assets/Materials con shader URP/Lit: " + materiales.Length);

            // Manipulables
            var grabs = UnityEngine.Object.FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);
            Check(grabs.Length >= 2, "XR Grab Interactable: " + grabs.Length + " objetos (minimo 2)");
            foreach (var g in grabs)
            {
                var rb = g.GetComponent<Rigidbody>();
                Check(rb != null && rb.useGravity, g.name + ": Rigidbody con Use Gravity");
                Check(g.throwOnDetach, g.name + ": Throw On Detach activado");
            }
            var mesa = GameObject.Find("Mesa_Tablero");
            Check(mesa != null && mesa.GetComponent<Collider>() != null,
                "Mesa con Collider (los objetos no caen al vacio)");

            // Interacción por rayo
            var simples = UnityEngine.Object.FindObjectsByType<XRSimpleInteractable>(FindObjectsSortMode.None);
            Check(simples.Length >= 2, "XR Simple Interactable: " + simples.Length + " (minimo 2)");
            var toggle = UnityEngine.Object.FindFirstObjectByType<ToggleLuz>();
            Check(toggle != null && toggle.luz != null,
                "Boton_Luz -> ToggleLuz conectado a la Point Light por codigo");
            var cambiar = UnityEngine.Object.FindFirstObjectByType<CambiarColor>();
            Check(cambiar != null && cambiar.objetivo != null,
                "Boton_Color -> CambiarColor conectado por codigo");

            // Teletransporte (el campo estatico Piso solo se llena en Build();
            // en batch que solo ejecuta Verify hay que buscarlo en la escena abierta)
            var pisoVerify = GameObject.Find("Piso");
            Check(pisoVerify != null && pisoVerify.GetComponent<TeleportationArea>() != null,
                "Teleportation Area en el piso");
            Check(UnityEngine.Object.FindFirstObjectByType<TeleportationProvider>() != null,
                "Teleportation Provider en el rig");

            // Contador
            var contador = UnityEngine.Object.FindFirstObjectByType<ContadorLuces>();
            Check(contador != null, "ContadorLuces presente");
            Check(toggle != null && toggle.contador != null && toggle.contador == contador,
                "Contador conectado al ToggleLuz por codigo");
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            Check(canvas != null && canvas.renderMode == RenderMode.WorldSpace,
                "Canvas del contador en World Space");
            Check(UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None).Length >= 1,
                "Texto TextMeshPro presente");

            // Simulador y unicidad
            Check(GameObject.Find("XR Device Simulator") != null,
                "XR Device Simulator disponible en la escena");
            Check(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length == 1,
                "Una sola camara en la escena");
            Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1,
                "Un solo EventSystem");
            Check(UnityEngine.Object.FindObjectsByType<XRInteractionManager>(FindObjectsSortMode.None).Length == 1,
                "Un solo XR Interaction Manager");

            if (_fallos.Count == 0)
            {
                Debug.Log("[XRChallenge] == VERIFICACION OK: todos los criterios cumplidos ==");
            }
            else
            {
                string resumen = string.Join("\n - ", _fallos);
                Debug.LogError($"[XRChallenge] == VERIFICACION CON FALLOS ({_fallos.Count}):\n - {resumen}");
                throw new Exception("Verificacion fallida: " + _fallos.Count + " criterios incumplidos.");
            }
        }
    }
}
