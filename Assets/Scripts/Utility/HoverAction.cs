using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverAction : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IDeselectHandler
{
	public UnityEvent onSelect;
	public UnityEvent onDeselect;

	private void Awake()
	{
		if (TryGetComponent(out HoverOverSelectable h))
		{
			onSelect.AddListener(h.SetHoveredScale);
			onDeselect.AddListener(h.GetDefaultScale);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		onSelect.Invoke();
	}

	public void OnSelect(BaseEventData eventData)
	{
		onSelect.Invoke();
	}
	public void OnDeselect(BaseEventData eventData)
	{
		onDeselect.Invoke();
	}
}
