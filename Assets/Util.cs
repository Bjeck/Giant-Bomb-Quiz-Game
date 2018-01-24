using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Util{

    private static readonly System.DateTime EpochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);




    public static int GetTimeInUnix()
    {
        return (int)(System.DateTime.UtcNow - EpochStart).TotalSeconds;
    }

#if UNITY_EDITOR
    public static void HeaderLabel(Rect space, string label, GUIStyle style, Color color){
		style.normal.textColor = color;
		EditorGUI.LabelField (space, label, style);
	}

	public static void DropShadowHeaderLabel(Rect space, string label, GUIStyle style, Color color){
		//Shadow
		Vector2 prevOff = style.contentOffset;
		style.contentOffset = new Vector2 (2f, 2f);
		style.normal.textColor = Color.black.WithAlpha(0.5f);
		EditorGUI.LabelField (space, label, style);

		//Label
		style.contentOffset = prevOff;
		style.normal.textColor = color;
		EditorGUI.LabelField (space, label, style);

	}

	public static void DrawOutlineRect(Rect r, Color fillColor, Color strokeColor, float strokewidth)
	{
		for (int i = 0; i < strokewidth; i++) {
			r.x -= 1;
			r.y -= 1;
			r.xMax += 2;
			r.yMax += 2;
			Handles.DrawSolidRectangleWithOutline (r, fillColor, strokeColor);
		}
	}

	#region Editor Util
	public static bool ResizableRegion(Rect region, MouseCursor resizeType, out float modifies, ref bool interacting){
		Event e = Event.current;
		modifies = 0f;
		EditorGUIUtility.AddCursorRect (region, resizeType);

		if (region.IsHovered ()) {
			EditorGUI.DrawRect (region, Color.white.WithAlpha (0.25f));
		}

		if (region.IsClicked ()) {
			interacting = true;
			e.Use ();
		}
		if (e.type == EventType.mouseUp) {
			interacting = false;
		}

		if (interacting) {
			switch (resizeType) {
			case MouseCursor.ResizeVertical:
				modifies = e.mousePosition.y;
				break;
			case MouseCursor.ResizeHorizontal:
				modifies = e.mousePosition.x;
				break;
			}
			return true;
		} else {
			return false;
		}
	}

	public static bool FlatButton(Rect rect, string label, Color color, GUIStyle style){
		EditorGUI.DrawRect (rect, color);
		EditorGUI.LabelField (rect, label, style);

		if (rect.IsClicked ()) {
			return true;
		} else {
			return false;
		}
	}

	#endregion

#endif

    

    public static Vector3 PosTo2D(Vector3 pos, Canvas c, bool world)
    {
        
        if (world)
        {
            Vector3 pos3d;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(c.transform as RectTransform, pos, c.worldCamera, out pos3d);
            //return c.transform.TransformPoint(pos3d);
            return Camera.main.ScreenToWorldPoint(pos3d);
        }
        else
        {
            Vector2 pos2d;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(c.transform as RectTransform, pos, c.worldCamera, out pos2d);
            return c.transform.TransformPoint(pos2d);
 
        }


    }

    public static Vector3 PosTo2DRect(RectTransform rt, Vector3 pos, Canvas c)
    {
        Vector3 globalMousePos;
        //Debug.Log(c.name + " " + rt.name);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(c.transform as RectTransform, pos, c.worldCamera, out globalMousePos))
        {
          //  Debug.Log(rt);
            
           // rt.position = globalMousePos;
            rt.rotation = ((RectTransform)c.transform).rotation;
        }
        return globalMousePos;
    }

    public static Vector3 PosTo2DRect(Transform rt, Vector3 pos, Canvas c)
    {
        Vector3 globalMousePos;
        //Debug.Log(c.name + " " + rt.name);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(c.transform as RectTransform, pos, c.worldCamera, out globalMousePos))
        {
            //  Debug.Log(rt);

            // rt.position = globalMousePos;
            rt.rotation = ((RectTransform)c.transform).rotation;
        }
        return globalMousePos;
    }


	/// <summary>
	/// These lerps function like regular Mathf.Lerp, except they don't clamp animation at 0-1, so I can overshoot and undershoot with animationCurves. Value must be a number between 0 and 1.
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <param name="value"></param>
	/// <param name="c"></param>
	/// <returns></returns>
	public static float UnClampedAnimationLerp(float start, float end, float value, AnimationCurve c)
	{
		return start + ((end - start) * c.Evaluate(value));
	}

	public static Vector3 UnClampedAnimationLerp(Vector3 start, Vector3 end, float value, AnimationCurve c)
	{
		return start + ((end - start) * c.Evaluate(value));
	}

	public static Vector2 UnClampedAnimationLerp(Vector2 start, Vector2 end, float value, AnimationCurve c)
	{
		return start + ((end - start) * c.Evaluate(value));
	}


	public static IEnumerator LerpFill(float old, float newnr, Image val, AnimationCurve c, float speed)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			val.fillAmount = UnClampedAnimationLerp(old, newnr, t, c);
			yield return new WaitForEndOfFrame();
		}
	}

	public static IEnumerator LerpNr(float old, float newnr, float val, AnimationCurve c, float speed)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			val = UnClampedAnimationLerp(old, newnr, t, c);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General coroutine for scaling a recttransform, from old to new, with animation curve.
	/// </summary>
	public static IEnumerator Scale(Vector3 oldS, Vector3 newS, RectTransform rt, AnimationCurve c, float speed)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			rt.transform.localScale = Util.UnClampedAnimationLerp(oldS, newS, t, c);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General coroutine for scaling a recttransform, from old to new, with animation curve, randomization, delay and speed possible
	/// </summary>
	public static IEnumerator Scale(Vector3 oldS, Vector3 newS, RectTransform rt, AnimationCurve c, float speed, float delay)
	{

		if (delay < 0)
		{
			yield return new WaitForSeconds(delay);
		}

		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			rt.transform.localScale = Util.UnClampedAnimationLerp(oldS, newS, t, c);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General coroutine for scaling a recttransform, from old to new, with animation curve, randomization, delay and speed possible
	/// </summary>
	public static IEnumerator Scale(Vector3 oldS, Vector3 newS, RectTransform rt, AnimationCurve c, float speed, float delay, float random)
	{

		if (delay < 0)
		{
			yield return new WaitForSeconds(delay);
		}

		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed + UnityEngine.Random.Range(-random,random);
			rt.transform.localScale = Util.UnClampedAnimationLerp(oldS, newS, t, c);
			yield return new WaitForEndOfFrame();
		}
	}




	/// <summary>
	/// General coroutine for scaling a Recttransform up and down.
	/// </summary>
	/// <param name="oldS"></param>
	/// <param name="newS"></param>
	/// <param name="rt"></param>
	/// <param name="c"></param>
	/// <param name="c2"></param>
	/// <param name="speed"></param>
	/// <returns></returns>
	public static IEnumerator ScaleUpAndDown(Vector3 oldS, Vector3 newS, RectTransform rt, AnimationCurve c, AnimationCurve c2, float speed)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			rt.transform.localScale = Util.UnClampedAnimationLerp(oldS, newS, t, c);
			yield return new WaitForEndOfFrame();
		}

		t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			rt.transform.localScale = Util.UnClampedAnimationLerp(newS, oldS, t, c2);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General coroutine for scaling a Recttransform up and down.
	/// </summary>
	/// <param name="oldS"></param>
	/// <param name="newS"></param>
	/// <param name="rt"></param>
	/// <param name="c"></param>
	/// <param name="c2"></param>
	/// <param name="speed"></param>
	/// <returns></returns>
	public static IEnumerator ScaleUpAndDown(Vector3 oldS, Vector3 newS, RectTransform rt, AnimationCurve c, AnimationCurve c2, float speed1, float speed2)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed1;
			rt.transform.localScale = Util.UnClampedAnimationLerp(oldS, newS, t, c);
			yield return new WaitForEndOfFrame();
		}

		t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed2;
			rt.transform.localScale = Util.UnClampedAnimationLerp(newS, oldS, t, c2);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General Coroutine for moving a Recttransform from one position to new position. with animation curve, and speed.
	/// </summary>
	public static IEnumerator MoveToPos(Vector2 oldPos, Vector2 newPos, RectTransform rt, AnimationCurve c, float speed)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			rt.anchoredPosition = UnClampedAnimationLerp(oldPos, newPos, t, c);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General Coroutine for moving a Recttransform from one position to new position. with animation curve, with delay.
	/// </summary>
	public static IEnumerator MoveToPos(Vector2 oldPos, Vector2 newPos, RectTransform rt, AnimationCurve c, float speed, float delay)
	{

		if (delay > 0)
		{
			yield return new WaitForSeconds(delay);
		}

		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed;
			rt.anchoredPosition = UnClampedAnimationLerp(oldPos, newPos, t, c);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General Coroutine for moving a Recttransform from one position to new position. with animation curve, with delay.
	/// </summary>
	public static IEnumerator MoveToPos(Vector2 oldPos, Vector2 newPos, RectTransform rt, AnimationCurve c, float speed, float delay, float random)
	{

		if (delay > 0)
		{
			yield return new WaitForSeconds(delay);
		}

		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed + UnityEngine.Random.Range(-random, random);
			rt.anchoredPosition = UnClampedAnimationLerp(oldPos, newPos, t, c);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General Coroutine for moving a Recttransform from one position to new position. with animation curve, and speed.
	/// </summary>
	public static IEnumerator MoveToPosAndBack(Vector2 oldPos, Vector2 newPos, RectTransform rt, AnimationCurve c1, AnimationCurve c2, float speed1, float speed2)
	{
		float t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed1;
			rt.anchoredPosition = UnClampedAnimationLerp(oldPos, newPos, t, c1);
			yield return new WaitForEndOfFrame();
		}

		t = 0;
		while (t <= 1)
		{
			t += Time.deltaTime * speed2;
			rt.anchoredPosition = UnClampedAnimationLerp(newPos, oldPos, t, c2);
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// General coroutine for rotating a Recttransform. input 360 for a complete rotation.
	/// </summary>
	/// <param name="rt"></param>
	/// <param name="c"></param>
	/// <param name="speed"></param>
	/// <returns></returns>
	public static IEnumerator Spin(RectTransform rt, AnimationCurve c, float speed, float targetAngle, bool random)
	{
		float t = 0;
		Quaternion startRot = rt.transform.rotation;
		float orgAngle;
		Vector3 orgAxis;
		startRot.ToAngleAxis(out orgAngle, out orgAxis);
        Debug.Log(orgAngle +" "+ targetAngle);

		float currentAngle;
		while (t < 1)
		{
			if (random)
			{
				t += Time.deltaTime + UnityEngine.Random.Range(-0.01f, 0.01f) * speed;
			}
			else
			{
				t += Time.deltaTime * speed;
			}


			currentAngle = UnClampedAnimationLerp(orgAngle, targetAngle, t, c);
			rt.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

			yield return null;
		}
		rt.transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
	}

    /// <summary>
    /// Spin that takes a start angle and a target angle, for specific values.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="c"></param>
    /// <param name="speed"></param>
    /// <param name="startAngle"></param>
    /// <param name="targetAngle"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    public static IEnumerator Spin(RectTransform rt, AnimationCurve c, float speed, float startAngle, float targetAngle, bool random)
    {
        float t = 0;
        Quaternion startRot = rt.transform.rotation;
        float orgAngle;
        Vector3 orgAxis;
        startRot.ToAngleAxis(out orgAngle, out orgAxis);

        float currentAngle;
        while (t < 1)
        {
            if (random)
            {
                t += Time.deltaTime + UnityEngine.Random.Range(-0.01f, 0.01f) * speed;
            }
            else
            {
                t += Time.deltaTime * speed;
            }


            currentAngle = UnClampedAnimationLerp(startAngle, targetAngle, t, c);
            rt.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

            yield return null;
        }
        rt.transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
    }

    /// <summary>
    /// Rotates briefly in a random direction, then rotates back.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RotateAskew(RectTransform rt, AnimationCurve c, float speed, float targetAngle)
	{
		float t = 0;
		Quaternion startRot = rt.transform.rotation;
		float orgAngle;
		Vector3 orgAxis;
		startRot.ToAngleAxis(out orgAngle, out orgAxis);

		float currentAngle;
		while (t < 1)
		{
			t += Time.deltaTime * speed;

			currentAngle = Util.UnClampedAnimationLerp(orgAngle, targetAngle, t, c);
			rt.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

			yield return null;
		}

		t = 0;
		while (t < 1)
		{
			t += Time.deltaTime * (speed/2f);

			currentAngle = Util.UnClampedAnimationLerp(targetAngle, orgAngle, t, c);
			rt.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

			yield return null;
		}

		rt.transform.rotation = startRot;
	}

    /// <summary>
    /// Rotates briefly in a random direction, then rotates back.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RotateAskew(RectTransform rt, AnimationCurve c, float speed, float startAngle, float targetAngle)
    {
        float t = 0;
        Quaternion startRot = rt.transform.rotation;
        float orgAngle;
        Vector3 orgAxis;
        startRot.ToAngleAxis(out orgAngle, out orgAxis);

        float currentAngle;
        while (t < 1)
        {
            t += Time.deltaTime * speed;

            currentAngle = Util.UnClampedAnimationLerp(startAngle, targetAngle, t, c);
            rt.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * (speed / 2f);

            currentAngle = Util.UnClampedAnimationLerp(targetAngle, startAngle, t, c);
            rt.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

            yield return null;
        }

        rt.transform.rotation = startRot;
    }

    /// <summary>
    /// Finds the outer edge position of a BOTTOM rect transform. the position where it is exactly just below the screen. Assumes its anchor/pivot is in the bottom
    /// </summary>
    /// <param name="rt"></param>
    /// <returns></returns>
    public static Vector2 BottomEdgePosition(RectTransform rt)
	{
		return new Vector2(0, -rt.sizeDelta.y);
	}

	public static IEnumerator WaitToDisable(GameObject g,float t) {
		yield return new WaitForSeconds(t);
		g.SetActive(false);
	}

    public static IEnumerator WaitToEnable(GameObject g, float t)
    {
        yield return new WaitForSeconds(t);
        g.SetActive(true);
    }


    public static float Map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}





	/// <summary>
	/// Created as a precursor to backend integration. When integration is ready, you can exchange these calls with plugin calls.
	/// </summary>
	/// <param name="path">The path to the resourceFile you wish to load.</param>
	/// <returns>Returns a string comprised of the collected data.</returns>
	public static string LoadDummyText(string path)
	{
		return Resources.Load<TextAsset>(path).text;
	}
	/// <summary>
	/// Created as a precursor to backend integration. When integration is ready, you can exchange these calls with plugin calls.
	/// </summary>
	/// <param name="path">The path to the resourceFile you wish to load.</param>
	/// <returns>Returns a sprite created from the collected texture.</returns>
	public static Sprite LoadDummySprite(string path)
	{
		Texture2D tex = Resources.Load<Texture2D>(path);
		return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
	}


	public static Vector3 HalfVector()
	{
		return Vector3.one * 0.5f;
	}









}


public static class ExtensionMethods{

	#region Color 

	public static Color WithAlpha(this Color color, float alpha){
		color.a = alpha;
		return color;
	}

	#endregion

	#region Vector3

	public static Vector3 WithMagnitude(this Vector3 v, float magnitude){
		return v.normalized * magnitude;
	}

	public static Vector3 WithY(this Vector3 v, float newY){
		return new Vector3 (v.x, newY, v.z); 
	}

    public static Vector3 WithZ(this Vector3 v, float newZ)
    {
        return new Vector3(v.x, v.y, newZ);
	}

	#endregion

	#region Rect

	public static Rect WithHorizontalPadding(this Rect rect, float padding){
		rect.x += padding;
		rect.xMax -= padding * 2;

		return rect;
	}

	public static Rect WithVerticalPadding(this Rect rect, float padding){
		rect.y += padding;
		rect.yMax -= padding * 2;

		return rect;
	}

    public static Rect WithPadding(this Rect rect, float padding){
		rect.x += padding;
		rect.xMax -= padding * 2;
		rect.y += padding;
		rect.yMax -= padding * 2;
		
		return rect;
	}

	public static Rect WithX(this Rect rect, float x){
		rect.xMin = x;
		return rect;
	}

	public static Rect WithY(this Rect rect, float y){
		rect.yMin = y;
		return rect;
	}

	public static Rect WithWidth(this Rect rect, float width){
		rect.width = width;
		return rect;
	}

	public static Rect WithHeight(this Rect rect, float height){
		rect.height = height;
		return rect;
	}

	public static Rect WithCenter(this Rect rect, Vector2 position){
		rect.x = position.x - rect.width / 2;
		rect.y = position.y - rect.height / 2;
		return rect;
	}

	public static Rect WithHorizontalCenter(this Rect rect, float x){
		rect.x = x - rect.width / 2;
		return rect;
	}

	public static Rect WithVerticalCenter(this Rect rect, float y){
		rect.y = y - rect.height / 2;
		return rect;
	}
		
	#endregion

	#region GUIStyle

	public static GUIStyle WithFontColor(this GUIStyle style, Color color){
		GUIStyle newStyle = new GUIStyle (style);
		newStyle.normal.textColor = color;
		return newStyle;
	}

	public static GUIStyle WithWordWrap(this GUIStyle style){
		GUIStyle newStyle = new GUIStyle (style);
		newStyle.wordWrap = true;
		return newStyle;
	}

	public static GUIStyle WithCenteredAlignment(this GUIStyle style){
		GUIStyle newStyle = new GUIStyle (style);
		newStyle.alignment = TextAnchor.MiddleCenter;
		return newStyle;
	}

	#endregion

	#region Event

	public static bool IsClicked(this Rect rect){
		Event e = Event.current;
		return rect.Contains (e.mousePosition) && e.type == EventType.mouseDown && e.button == 0;
	}

	public static bool IsRightClicked(this Rect rect){
		Event e = Event.current;
		return rect.Contains (e.mousePosition) && e.type == EventType.mouseDown && e.button == 1;
	}

	public static bool IsHovered(this Rect rect){
		Event e = Event.current;
		return rect.Contains (e.mousePosition);
	}

	#endregion
}

#region Better Rect classes

public abstract class InteractableRect{
	public Rect rect;

	public virtual void OnGUI(){
		Event e = Event.current;

		HandleInteractionEvents (e);
	}

	void HandleInteractionEvents(Event e){
		if (rect.IsClicked ()) {
			OnInteract (e);
		}

		if (rect.IsRightClicked ()) {
			OnRightClick (e);
		}

		if (e.type == EventType.mouseUp) {
			
			if (rect.Contains (e.mousePosition)) {
				OnMouseUpOverRect (e);
			}

			OnStopInteract (e);
		}

	}

	protected virtual void OnInteract(Event e){	}

	protected virtual void OnRightClick(Event e){ }

	protected virtual void OnStopInteract(Event e){	}

	protected virtual void OnMouseUpOverRect(Event e){ }
}

public abstract class DraggableRect : InteractableRect, ISelectableUIElement{
	public bool selected;

	private bool dragging;
	private Vector2 clickOffset = Vector2.zero;

	public override void OnGUI(){
		base.OnGUI ();

		Event e = Event.current;

		if (dragging) {
			Vector2 newPos = 
				e.mousePosition - 
				new Vector2 (rect.width * clickOffset.x, rect.height * clickOffset.y);  
			rect.position = newPos;
		}
	}

	protected override void OnInteract(Event e){
		base.OnInteract (e);

		dragging = true;
		OnSelect ();
		clickOffset = Rect.PointToNormalized (rect, e.mousePosition);
	}

	protected override void OnStopInteract(Event e){
		base.OnStopInteract (e);

		dragging = false;

		if (selected && !rect.Contains (e.mousePosition)) {
			OnDeselect ();
			GUI.FocusControl ("");
		}
	}

	public void OnSelect(){ 
		selected = true;
	}
	public void OnDeselect(){ 
		selected = false;
	}

	public virtual void OnInspectorGUI(Rect space){

	}
}

public abstract class ResizableRect : DraggableRect {

	public float resizingHandleSize = 5f;
	public float minSize = 35f;

	bool resizingTop, resizingBottom, resizingLeft, resizingRight;

	public override void OnGUI(){
		base.OnGUI ();
		HandleResizing ();
	}

	void HandleResizing(){
		float resizingHandleSize = 5f;

		Rect top = rect.WithY(rect.yMin - (resizingHandleSize+1)).WithHeight (resizingHandleSize);
		float newYMin = 0f;
		if (Util.ResizableRegion (top, MouseCursor.ResizeVertical, out newYMin, ref resizingTop)) {
			rect.yMin = newYMin < rect.yMax - minSize ? newYMin : rect.yMax-minSize;
		}

		Rect bottom = rect.WithY (rect.yMax + 1f).WithHeight (resizingHandleSize);
		float newYMax = 0f;
		if (Util.ResizableRegion (bottom, MouseCursor.ResizeVertical, out newYMax, ref resizingBottom)) {
			rect.yMax = newYMax > rect.yMin + minSize ? newYMax : rect.yMin + minSize;
		}

		Rect left = rect.WithX (rect.xMin - (resizingHandleSize+1)).WithWidth (resizingHandleSize);
		float newXMin = 0f;
		if (Util.ResizableRegion (left, MouseCursor.ResizeHorizontal, out newXMin, ref resizingLeft)) {
			rect.xMin = newXMin < rect.xMax - minSize ? newXMin : rect.xMax - minSize;
		}

		Rect right = rect.WithX (rect.xMax + 1f).WithWidth (resizingHandleSize);
		float newXMax = 0f;
		if (Util.ResizableRegion (right, MouseCursor.ResizeHorizontal, out newXMax, ref resizingRight)) {
			rect.xMax = newXMax > rect.xMin + minSize ? newXMax : rect.xMin + minSize;
		}
	}
}

public interface ISelectableUIElement{

	void OnSelect();
	void OnDeselect();

	void OnInspectorGUI (Rect space);
}

#endregion