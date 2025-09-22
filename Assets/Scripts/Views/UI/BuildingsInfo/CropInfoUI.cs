using Models.Work;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Views.Ui.Buildings;

public class CropInfoUI : BuildingInfoUI
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text progressLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Slider progressSlider;

    private CropModel cropModel;

    private void OnEnable() => cropModel.OnValueChanged += RefreshUI;
    private void OnDisable() => cropModel.OnValueChanged -= RefreshUI;

    public void Init(Transform buildingTransform, CropModel cropModel)
    {
        this.buildingTransform = buildingTransform;
        this.cropModel = cropModel;

        gameObject.SetActive(true);
        RefreshUI();
    }

    private void RefreshUI()
    {
        nameLabel.text = cropModel.Name;
        statusLabel.text = cropModel.CropFieldState.ToString();

        progressLabel.text = $"Progress: {Mathf.RoundToInt(cropModel.GrowthProgress * 100)}%";
        progressSlider.value = cropModel.GrowthProgress;
    }
}
