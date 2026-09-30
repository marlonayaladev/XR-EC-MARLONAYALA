// Etapa 5 (reto libre): contador visible en un Canvas World Space con
// TextMeshPro que indica cuántas veces se encendió la luz interactiva.
using TMPro;
using UnityEngine;

namespace XRChallenge
{
    public class ContadorLuces : MonoBehaviour
    {
        TextMeshProUGUI _texto;
        int _encendidos;

        void Awake()
        {
            // Busca el TMP dentro del propio Canvas (World Space).
            _texto = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        public void SumarEncendido()
        {
            _encendidos++;
            Actualizar();
        }

        void Actualizar()
        {
            if (_texto != null)
                _texto.text = $"Veces encendida: {_encendidos}";

            Debug.Log($"[XRChallenge] Contador de luz: {_encendidos}");
        }
    }
}
