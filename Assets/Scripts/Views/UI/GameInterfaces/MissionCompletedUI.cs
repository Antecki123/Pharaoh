using System.Collections;
using UnityEngine;

namespace Views.Ui.GameInterfaces
{
    public class MissionCompletedUI : MonoBehaviour
    {
        [SerializeField] private float duration = 2f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(duration);
            gameObject.SetActive(false);
        }
    }
}