using System.Linq;
using CUCoreLib;
using CUCoreLib.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class PatchMenu : MonoBehaviour
{
    private static GameObject goInit;
    private static readonly Sprite spriteTable = AssetLoader.LoadEmbeddedSprite("Assets.MenuButton.Tablet_Menu.png");

    private GameObject menuBackground;
    private RectTransform tableButtonRect;
    private MenuSpriteHitbox tableHitbox;
    private Image tableImage;
    private TextMeshProUGUI tableText;
    private Texture2D outlineTexture;
    private Image tableOutline;
    private Sprite outlineSprite;

    public static void Patch()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private static void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.name == "PreGen")
        {
            if (goInit == null)
            {
                goInit = new GameObject("PatchMenuInitializer");
                goInit.AddComponent<PatchMenu>();
            }
        }
        else
        {
            if (goInit != null)
            {
                Destroy(goInit);
                goInit = null;
            }
        }
    }

    private void Awake()
    {
        menuBackground = GameObject.Find("MenuBackground");
    }

    private void Start()
    {
        CreateMenuButton();
    }
    private void Update()
    {
        if (tableHitbox == null || tableButtonRect == null)
            return;

        // Ховер проверяем вручную, как в оригинале
        bool hovered = tableHitbox.IsRaycastLocationValid(Input.mousePosition, null);

        // Клик
        if (hovered && Input.GetMouseButtonDown(0))
        {
            OnButtonClicked();
            Input.ResetInputAxes();
        }

        // Дистанция от курсора до кнопки (для свечения "при приближении")
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tableButtonRect, Input.mousePosition, null, out Vector2 local);
        Rect rect = tableButtonRect.rect;
        Vector2 clamped = new Vector2(
            Mathf.Clamp(local.x, rect.xMin, rect.xMax),
            Mathf.Clamp(local.y, rect.yMin, rect.yMax));
        float dist = hovered ? 0f
            : (rect.Contains(local) ? 80f : Vector2.Distance(local, clamped));

        float fade = Mathf.Clamp01(Time.unscaledDeltaTime * 3f); // плавность

        // Текст: проявляем при ховере
        if (tableText != null)
        {
            Color tc = tableText.color;
            tc.a = Mathf.Lerp(tc.a, hovered ? 1f : 0f, fade);
            tableText.color = tc;
        }

        // Рамка: светится тем сильнее, чем ближе курсор
        if (tableOutline != null)
        {
            float target = Mathf.Max(
                Mathf.Clamp01(1f - dist * 0.005f) * 0.52f, 0.025f);
            Color oc = tableOutline.color;
            oc.a = Mathf.Lerp(oc.a, target, fade);
            tableOutline.color = oc;
        }
    }

    private void OnButtonClicked()
    {
        // Используем Debug.Log вместо Console.WriteLine, чтобы видеть логи в Unity/консоли BepInEx
        Debug.Log("[PatchMenu] Кнопка Cashout Drop нажата!");
        
        // ==========================================
        // ЗДЕСЬ ТВОЯ ЛОГИКА
        // Например:
        // MyMod.OpenWindow();
        // SceneManager.LoadScene("MyScene");
        // ==========================================
    }

    private void CreateMenuButton()
    {
        PreRunScript menu = PreRunScript.instance;
        if (menu == null || menu.mainCanvas == null || menuBackground == null)
            return;

        AdaptiveButton templateButton = menu.mainCanvas
            .GetComponentsInChildren<AdaptiveButton>(true)
            .FirstOrDefault(x => x.action == AdaptiveButton.MenuAction.Settings) 
            ?? menu.mainCanvas.GetComponentsInChildren<AdaptiveButton>(true).FirstOrDefault();

        // 1. Root Button
        GameObject buttonObject = new GameObject("TableCashoutMenuButton", typeof(RectTransform), typeof(MenuSpriteHitbox));
        
        // Привязываем к MenuBackground, а не к Canvas
        buttonObject.transform.SetParent(menuBackground.transform, false);

        // 2. RectTransform
        tableButtonRect = buttonObject.GetComponent<RectTransform>();
        tableButtonRect.anchorMin = new Vector2(0.5f, 0f);
        tableButtonRect.anchorMax = new Vector2(0.5f, 0f);
        tableButtonRect.pivot = new Vector2(0.5f, 0.5f);
        tableButtonRect.anchoredPosition = new Vector2(300f, 80f); // Твои координаты
        tableButtonRect.sizeDelta = new Vector2(350f, 146f);

        // Настраиваем порядок отрисовки (Sibling Index), чтобы не перекрывать титры
        PlaceBelowMenuCover(menu, templateButton);

        // 3. Hitbox
        tableHitbox = buttonObject.GetComponent<MenuSpriteHitbox>();
        tableHitbox.color = Color.clear;
        tableHitbox.raycastTarget = true;

        // 4. Artwork (Image)
        GameObject artworkObject = new GameObject("Artwork", typeof(RectTransform), typeof(Image));
        artworkObject.transform.SetParent(buttonObject.transform, false);
        
        tableImage = artworkObject.GetComponent<Image>();
        tableImage.raycastTarget = true;
        
        RectTransform artworkRect = tableImage.rectTransform;
        artworkRect.anchorMin = Vector2.zero;
        artworkRect.anchorMax = Vector2.one;
        artworkRect.offsetMin = Vector2.zero;
        artworkRect.offsetMax = Vector2.zero;

        tableImage.sprite = spriteTable;
        tableImage.type = Image.Type.Simple;
        tableImage.preserveAspect = true;

        tableHitbox.sprite = tableImage.sprite;
        tableHitbox.type = Image.Type.Simple;
        tableHitbox.preserveAspect = true;
        tableHitbox.UseTextureAlpha = spriteTable != null;

        // 5. Label (Text)
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        tableText = labelObject.GetComponent<TextMeshProUGUI>();
        tableText.text = "Cashout Setting";
        tableText.alignment = TextAlignmentOptions.Center;
        tableText.enableWordWrapping = false;
        tableText.overflowMode = TextOverflowModes.Ellipsis;
        tableText.raycastTarget = false;

        if (templateButton != null && templateButton.text != null)
        {
            tableText.font = templateButton.text.font;
            tableText.fontSize = templateButton.text.fontSize;
            tableText.fontStyle = templateButton.text.fontStyle;
            tableText.color = templateButton.text.color;
        }
        else
        {
            tableText.font = TMP_Settings.defaultFontAsset;
            tableText.fontSize = 24f;
            tableText.fontStyle = FontStyles.Bold;
        }

        RectTransform labelRect = tableText.rectTransform;
        tableText.transform.position += Vector3.right * 2;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(30f, 5f);
        labelRect.offsetMax = new Vector2(-30f, -5f);

        // 6. Parallax
        RegisterWithMenuParallax(templateButton);

        if (spriteTable != null)
            CreateOutline(spriteTable);

        // // 7 ButtonOnClick
        // Button tableButton = buttonObject.AddComponent<Button>();
        // tableButton.targetGraphic = tableHitbox; 

        // ColorBlock colors = tableButton.colors;
        // colors.normalColor = new Color(1f, 1f, 1f, 1f);           // Обычное состояние
        // colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f); // При наведении мыши (чуть ярче)
        // colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);     // При клике (чуть темнее)
        // colors.fadeDuration = 0.1f;                                 // Скорость перехода цвета
        // tableButton.colors = colors;

        // // 4. Добавляем сам обработчик нажатия
        // tableButton.onClick.AddListener(() => 
        // {
        //     Console.WriteLine("[PatchMenu] Кнопка Cashout Drop нажата!");
        // });
    }

    private void PlaceBelowMenuCover(PreRunScript menu, AdaptiveButton template)
    {
        if (tableButtonRect == null || menuBackground == null) return;

        // Ищем кнопку Quit (Выход), чтобы встать рядом с ней в иерархии.
        AdaptiveButton quitButton = menu.mainCanvas.GetComponentsInChildren<AdaptiveButton>(true)
            .FirstOrDefault(x => x.action == AdaptiveButton.MenuAction.Quit);

        if (quitButton != null)
        {
            Transform quitTransform = quitButton.transform;
            // Поднимаемся по иерархии, пока не найдем прямого чайлда MenuBackground
            while (quitTransform != null && quitTransform.parent != menuBackground.transform)
            {
                quitTransform = quitTransform.parent;
            }

            if (quitTransform != null)
            {
                // Ставим нашу кнопку на тот же уровень (или чуть ниже/выше, зависит от Canvas)
                tableButtonRect.transform.SetSiblingIndex(quitTransform.GetSiblingIndex());
                return;
            }
        }

        // Если не нашли Quit, просто кидаем в начало, чтобы не перекрывать титры
        tableButtonRect.transform.SetAsFirstSibling();
    }

    private void RegisterWithMenuParallax(AdaptiveButton template)
    {
        MenuParallax menuParallax = MenuParallax.instance;
        if (menuParallax?.parallax?.layers == null || tableButtonRect == null) return;

        float mouseMoveScale = 0.08f;

        if (template != null)
        {
            RectTransform templateRect = template.GetComponent<RectTransform>();
            MenuParallax.ParallaxLayerInfo existingLayer = menuParallax.parallax.layers
                .FirstOrDefault(layer => layer.reference == templateRect || (layer.reference != null && templateRect.IsChildOf(layer.reference)));

            if (existingLayer != null) mouseMoveScale = existingLayer.mouseMoveScale;
        }
        else if (menuParallax.parallax.layers.Count > 0)
        {
            mouseMoveScale = menuParallax.parallax.layers[menuParallax.parallax.layers.Count - 1].mouseMoveScale;
        }

        menuParallax.parallax.layers.Add(new MenuParallax.ParallaxLayerInfo
        {
            reference = tableButtonRect,
            originalPos = tableButtonRect.anchoredPosition,
            mouseMoveScale = mouseMoveScale
        });
    }
    private void CreateOutline(Sprite source)
    {
        try
        {
            Rect texRect = source.textureRect;
            int w = Mathf.RoundToInt(texRect.width);
            int h = Mathf.RoundToInt(texRect.height);
            int ox = Mathf.RoundToInt(texRect.x);
            int oy = Mathf.RoundToInt(texRect.y);
            if (w <= 0 || h <= 0) return;

            // Белый силуэт: цвет = белый, альфа = альфа оригинала
            Color32[] srcPixels = source.texture.GetPixels32();
            Color32[] whitePixels = new Color32[w * h];
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    whitePixels[i * w + j] = new Color32(255, 255, 255,
                        srcPixels[(oy + i) * source.texture.width + ox + j].a);

            outlineTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outlineTexture.filterMode = FilterMode.Point;
            outlineTexture.wrapMode = TextureWrapMode.Clamp;
            outlineTexture.SetPixels32(whitePixels);
            outlineTexture.Apply(false);

            outlineSprite = Sprite.Create(outlineTexture,
                new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);

            GameObject outlineObj = new GameObject("ArtworkOutline",
                typeof(RectTransform), typeof(Image));
            outlineObj.transform.SetParent(tableButtonRect.transform, false);
            outlineObj.transform.SetAsFirstSibling(); // рамка ПОД картинкой

            tableOutline = outlineObj.GetComponent<Image>();
            tableOutline.sprite = outlineSprite;
            tableOutline.type = Image.Type.Simple;
            tableOutline.preserveAspect = true;
            tableOutline.raycastTarget = false;
            tableOutline.color = new Color(1f, 1f, 1f, 0.025f); // почти невидима

            RectTransform rt = tableOutline.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-1f, -1f); // чуть больше кнопки
            rt.offsetMax = new Vector2(1f, 1f);
        }
        catch
        {
            tableOutline = null; // текстура не читаема — пропускаем рамку
        }
    }
}

