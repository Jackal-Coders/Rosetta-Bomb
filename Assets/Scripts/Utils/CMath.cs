using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CMath {

	/// <summary>
	/// Rounds the float value to specificed number of places
	/// </summary>
	/// <param name="val"></param>
	/// <param name="places"></param>
	/// <returns></returns>
	public static float RoundToPoint(float val, int places) {
		val *= Mathf.Pow(10, places);
		val = Mathf.Round(val);
		val /= Mathf.Pow(10, places);
		return val;
	}

	/// <summary>
	/// Uses the RoundToPoint(float val, int places) on each component of the Vector3
	/// </summary>
	/// <param name="val"></param>
	/// <param name="places"></param>
	/// <returns></returns>
	public static Vector3 RoundToPoint(Vector3 val, int places) {
		return new Vector3(RoundToPoint(val.x, places), RoundToPoint(val.y, places), RoundToPoint(val.z, places));
	}

	/// <summary>
	/// Replacement for Mathf.Clamp that does not force reassignment of the value
	/// </summary>
	/// <param name="value"></param>
	/// <param name="min"></param>
	/// <param name="max"></param>
	public static void Clamp(ref float value, float min, float max) {
		value = Mathf.Clamp(value, min, max);
	}

	/// <summary>
	/// Replacement for the Mathf.Clamp01 that does not force reassignment of the value
	/// </summary>
	/// <param name="value"></param>
	public static void Clamp01(ref float value) {
		value = Mathf.Clamp01(value);
	}
}
