using UnityEngine;
using TMPro;

public class ShipUI : MonoBehaviour
{
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;

    private ShipData data;
    private Camera mainCam;

    void Start()
    {
        data = GetComponentInParent<ShipData>();
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        // Toujours orienter vers la caméra
        if (mainCam != null)
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                             mainCam.transform.rotation * Vector3.up);
    }

    public void UpdateDisplay()
    {
        if (data == null) return;

        foodText.text = $"🍖 {data.food}";
        woodText.text = $"🪵 {data.wood}";
        stoneText.text = $"🪨 {data.stone}";
    }
}