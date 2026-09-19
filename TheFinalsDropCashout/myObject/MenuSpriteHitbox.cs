using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Кастомный компонент хитбокса на базе UI Image для более точной проверки попадания луча (Raycast) 
/// с учетом прозрачности текстуры спрайта или формы кнопки.
/// </summary>
internal sealed class MenuSpriteHitbox : Image
{
	/// <summary>Флаг использования альфа-канала текстуры для проверки попадания мыши.</summary>
	internal bool UseTextureAlpha;

	/// <summary>Порог прозрачности (альфа-канала), при котором клик считается действительным.</summary>
	internal float AlphaThreshold = 0.1f;

	/// <summary>
	/// Проверяет, попадает ли точка экрана в валидную область хитбокса спрайта.
	/// </summary>
	/// <param name="screenPoint">Точка экрана для проверки.</param>
	/// <param name="eventCamera">Камера событий UI.</param>
	/// <returns>True, если точка находится в пределах активной области хитбокса.</returns>
	public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out var localPoint))
		{
			return false;
		}
		Rect rect = base.rectTransform.rect;
		float num = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
		float num2 = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
		if (num < 0f || num > 1f || num2 < 0f || num2 > 1f)
		{
			return false;
		}
		if (UseTextureAlpha && base.sprite != null && base.sprite.texture != null)
		{
			try
			{
				Rect textureRect = base.sprite.textureRect;
				if (base.preserveAspect && rect.width > 0f && rect.height > 0f && textureRect.width > 0f && textureRect.height > 0f)
				{
					float num3 = textureRect.width / textureRect.height;
					float num4 = rect.width / rect.height;
					if (num3 > num4)
					{
						float num5 = rect.width / num3;
						float num6 = (rect.height - num5) * 0.5f;
						float num7 = localPoint.y - rect.yMin;
						if (num7 < num6 || num7 > num6 + num5)
						{
							return false;
						}
						num2 = (num7 - num6) / num5;
					}
					else
					{
						float num8 = rect.height * num3;
						float num9 = (rect.width - num8) * 0.5f;
						float num10 = localPoint.x - rect.xMin;
						if (num10 < num9 || num10 > num9 + num8)
						{
							return false;
						}
						num = (num10 - num9) / num8;
					}
				}
				float u = (textureRect.x + num * textureRect.width) / (float)base.sprite.texture.width;
				float v = (textureRect.y + num2 * textureRect.height) / (float)base.sprite.texture.height;
				return base.sprite.texture.GetPixelBilinear(u, v).a >= AlphaThreshold;
			}
			catch
			{
				UseTextureAlpha = false;
			}
		}
		float num11 = Mathf.Abs(num * 2f - 1f);
		float num12 = Mathf.Abs(num2 * 2f - 1f);
		if (num11 <= 1f && num12 <= 1f)
		{
			return num11 + num12 * 0.34f <= 1.13f;
		}
		return false;
	}

}