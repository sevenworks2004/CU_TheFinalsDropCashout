

using UnityEngine;
using UnityEngine.UI;

public class UIProgressCircle : MonoBehaviour
{
    public Transform targetCashout;

    private Image blueProgressCircle;
    private Text centerText; 
    private GameObject canvasObjProgress;


    public void Awake()
    {
        InitCanvasProgress();
    }

    public void InitCanvasProgress()
    {
        // 1. Создаем Canvas над объектом
        canvasObjProgress = new GameObject("CanvasProgress");

        canvasObjProgress.transform.SetParent(targetCashout, false);

        canvasObjProgress.transform.localPosition = new Vector3(0f, 7f, 0f); // выше объекта

        Canvas canvas = canvasObjProgress.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100; // На передний план перед объектами

        // Добавляем CanvasGroup для управления прозрачностью
        var canGroup = canvasObjProgress.AddComponent<CanvasGroup>();
        
        RectTransform canvasRT = canvasObjProgress.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(2f, 2f); // Размер холста 2x2 метра
        canvasObjProgress.transform.localScale = Vector3.one;

        // Создаем круглую белую текстуру в коде (чтобы не использовать внешний спрайт)
        Sprite circleSprite = CreateCircleSprite();

        // 2. ЖЕЛТЫЙ КРУГ (Большой фоновый круг)
        GameObject yellowCircleObj = new GameObject("YellowBgCircle");
        yellowCircleObj.transform.SetParent(canvasObjProgress.transform, false);

        Image yellowImage = yellowCircleObj.AddComponent<Image>();
        yellowImage.sprite = circleSprite;
        yellowImage.color = Color.yellow; // Желтый цвет

        RectTransform yellowRT = yellowCircleObj.GetComponent<RectTransform>();
        yellowRT.sizeDelta = new Vector2(1.8f, 1.8f); // Размер

        // 3. СИНИЙ КРУГ (Прогресс-бар поверх желтого)
        GameObject blueCircleObj = new GameObject("BlueProgressCircle");
        blueCircleObj.transform.SetParent(canvasObjProgress.transform, false);

        blueProgressCircle = blueCircleObj.AddComponent<Image>();
        blueProgressCircle.sprite = circleSprite;
        blueProgressCircle.color = Color.blue; // Синий цвет

        // Настройка заполнения круга по часовой стрелке
        blueProgressCircle.type = Image.Type.Filled;
        blueProgressCircle.fillMethod = Image.FillMethod.Radial360; // Круговая заливка 360 градусов
        blueProgressCircle.fillOrigin = (int)Image.Origin360.Top;   // Начинаем заливку с самого верха
        blueProgressCircle.fillAmount = 0f;                         // Изначально 0%

        RectTransform blueRT = blueCircleObj.GetComponent<RectTransform>();
        blueRT.sizeDelta = new Vector2(1.5f, 1.5f); // Чуть БОЛЬШЕ желтого, чтобы выступал поверх/вокруг

        // 4. ТЕКСТ "A" В ЦЕНТРЕ
        GameObject textObj = new GameObject("CenterText");
        textObj.transform.SetParent(canvasObjProgress.transform, false);

        centerText = textObj.AddComponent<Text>();
        centerText.text = "A";
        centerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        centerText.fontSize = 32;
        centerText.alignment = TextAnchor.MiddleCenter;
        centerText.color = Color.black;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(1f, 1f);
        textRT.anchoredPosition = Vector2.zero; // Ровно по центру

        canGroup.alpha = 0f;
        canGroup.interactable = false;
        canGroup.blocksRaycasts = false;
    }
    public void SetProgress(int value)
    {
        // Ограничиваем значение в диапазоне от 0 до 100
        int clampedValue = Mathf.Clamp(value, 0, 100);

        if (blueProgressCircle != null)
        {
            blueProgressCircle.fillAmount = clampedValue / 100f;
        }
        var canvasGroup = canvasObjProgress.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    private Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                colors[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    public void setVisibel(bool visibel)
    {
        if (visibel)
        {
            if (canvasObjProgress != null)
            {
                var canvasGroup = canvasObjProgress.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }
            
        }
        else
        {
            if (canvasObjProgress != null)
            {
                var canvasGroup = canvasObjProgress.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }
    }
}