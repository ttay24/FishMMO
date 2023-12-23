﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FishMMO.Shared;

namespace FishMMO.Client
{
	public abstract class UIControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public static readonly Color DEFAULT_COLOR = Hex.ColorNormalize(0.0f, 160.0f, 255.0f, 255.0f);
		public static readonly Color DEFAULT_SELECTED_COLOR = Hex.ColorNormalize(0.0f, 255.0f, 255.0f, 255.0f);

		public CanvasScaler CanvasScaler;
		public RectTransform MainPanel = null;
		[Tooltip("Helper field to check input field focus status in UIManager.")]
		public TMP_InputField InputField = null;
		public bool StartOpen = true;
		public bool IsAlwaysOpen = false;
		public bool HasFocus = false;
		public bool CloseOnQuitToMenu = true;

		[Header("Drag")]
		public bool CanDrag = false;
		public bool ClampToScreen = true;
		private Vector2 startPosition;
		private Vector2 dragOffset = Vector2.zero;
		private bool isDragging;

		public Client Client { get; private set; }
		public string Name { get { return gameObject.name; } set { gameObject.name = value; } }
		public bool Visible
		{
			get
			{
				return gameObject.activeSelf;
			}
			private set
			{
				gameObject.SetActive(value);
				if (!value && HasFocus)
				{
					EventSystem.current.SetSelectedGameObject(null);
					EventSystem.current.sendNavigationEvents = false;
					HasFocus = false;
				}
			}
		}

		private void Awake()
		{
			CanvasScaler = GetComponentInParent<CanvasScaler>();

			startPosition = transform.position;

			if (MainPanel == null)
			{
				MainPanel = transform as RectTransform;
			}

			//AdjustPositionForPivotChange(MainPanel, new Vector2(0.5f, 0.5f));
			//AdjustPositionForAnchorChange(MainPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
		}

		/* WIP - Pivot change works but Anchor does not.
		public void AdjustPositionForPivotChange(RectTransform rectTransform, Vector2 newPivot)
		{
			// Store the original values
			Vector3 originalPosition = rectTransform.localPosition;
			Vector2 originalSizeDelta = rectTransform.sizeDelta;
			Vector2 originalPivot = rectTransform.pivot;

			// Change the pivot
			rectTransform.pivot = newPivot;

			// Adjust localPosition based on pivot change
			Vector2 pivotDelta = new Vector2(newPivot.x - originalPivot.x, newPivot.y - originalPivot.y);
			rectTransform.localPosition = originalPosition + Vector3.Scale(pivotDelta, originalSizeDelta);
		}

		public void AdjustPositionForAnchorChange(RectTransform rectTransform, Vector2 newAnchorMin, Vector2 newAnchorMax)
		{
			// Store the original values
			Vector3 originalPosition = rectTransform.localPosition;
			Vector2 originalSizeDelta = rectTransform.sizeDelta;
			Vector2 originalAnchorMin = rectTransform.anchorMin;
			Vector2 originalAnchorMax = rectTransform.anchorMax;

			// Change the anchor
			rectTransform.anchorMin = newAnchorMin;
			rectTransform.anchorMax = newAnchorMax;

			// Calculate the differences in anchor positions
			Vector2 anchorMinDifference = newAnchorMin - originalAnchorMin;
			Vector2 anchorMaxDifference = newAnchorMax - originalAnchorMax;

			// Adjust localPosition based on anchor change
			Vector3 newPosition = originalPosition - Vector3.Scale(anchorMinDifference, originalSizeDelta) - Vector3.Scale(anchorMaxDifference, originalSizeDelta);
			rectTransform.localPosition = newPosition;
		}*/

		private void Start()
		{
			UIManager.Register(this);

			OnStarting();

			if (!StartOpen)
			{
				Hide();
			}
		}

		public virtual void OnQuitToLogin()
		{
		}

		private void Client_OnQuitToLogin()
		{
			Visible = !CloseOnQuitToMenu;
			OnQuitToLogin();
		}

		/// <summary>
		/// Dependency injection for the Client.
		/// </summary>
		public void SetClient(Client client)
		{
			Client = client;
			Client.OnQuitToLogin += Client_OnQuitToLogin;
		}

		/// <summary>
		/// Called at the start of the MonoBehaviour Start function.
		/// </summary>
		public abstract void OnStarting();

		private void OnDestroy()
		{
			Client.OnQuitToLogin -= Client_OnQuitToLogin;
			OnDestroying();

			UIManager.Unregister(this);
		}

		/// <summary>
		/// Called at the start of the MonoBehaviour OnDestroy function.
		/// </summary>
		public abstract void OnDestroying();

		public void OnPointerEnter(PointerEventData eventData)
		{
			HasFocus = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			HasFocus = false;
		}

		public virtual void ToggleVisibility()
		{
			Visible = !Visible;
		}

		public virtual void Show()
		{
			if (Visible)
			{
				return;
			}
			Visible = true;
		}

		public virtual void Hide()
		{
			Hide(IsAlwaysOpen);
		}

		public virtual void Hide(bool overrideIsAlwaysOpen)
		{
			if (overrideIsAlwaysOpen)
			{
				Show();
				return;
			}
			Visible = false;
		}

		public virtual void OnResetPosition()
		{
			transform.position = startPosition;
		}


		public void OnPointerDown(PointerEventData data)
		{
			if (!CanDrag) return;

			if (data != null)
			{
				if (MainPanel != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(MainPanel, data.pressPosition, data.pressEventCamera, out dragOffset))
				{
					if (CanvasScaler != null &&
						CanvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
					{
						dragOffset.x *= CanvasScaler.transform.localScale.x;
						dragOffset.y *= CanvasScaler.transform.localScale.y;
					}

					isDragging = true;
				}
				else
				{
					dragOffset = Vector2.zero;
				}
			}
		}

		public void OnPointerUp(PointerEventData data)
		{
			if (!CanDrag) return;

			isDragging = false;
		}

		public void OnDrag(PointerEventData data)
		{
			if (!CanDrag) return;

			if (isDragging)
			{
				float x = data.position.x - dragOffset.x;
				float y = data.position.y - dragOffset.y;
				if (ClampToScreen)
				{
					if (MainPanel != null)
					{
						float halfWidth = MainPanel.rect.width * 0.5f;
						float halfHeight = MainPanel.rect.height * 0.5f;

						if (CanvasScaler != null &&
							CanvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
						{
							halfWidth *= CanvasScaler.transform.localScale.x;
							halfHeight *= CanvasScaler.transform.localScale.y;
						}

						x = Mathf.Clamp(x, halfWidth, Screen.width - halfWidth);
						y = Mathf.Clamp(y, halfHeight, Screen.height - halfHeight);
					}
				}
				transform.position = new Vector2(x, y);
			}
		}

		public void ResetPosition()
		{
			transform.position = startPosition;
			dragOffset = Vector2.zero;
			isDragging = false;
		}

		/*public virtual void OnButtonEnter()
		{
			AudioClip clip;
			if (InternalResourceCache.TryGetAudioClip("uibeep", out clip))
			{
				GUIAudioSource.PlayOneShot(clip);
			}
		}

		public virtual void OnButtonClick()
		{
			AudioClip clip;
			if (InternalResourceCache.TryGetAudioClip("uibeep", out clip))
			{
				GUIAudioSource.PlayOneShot(clip);
			}
		}*/
	}
}