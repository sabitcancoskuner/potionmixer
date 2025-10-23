using UnityEngine;

public class Utils
{
   public static Vector3 ScreenToWorldPoint(Camera cam, Vector3 screenPosition)
   {
        screenPosition.z = cam.nearClipPlane;
       return cam.ScreenToWorldPoint(screenPosition);
   }
}
