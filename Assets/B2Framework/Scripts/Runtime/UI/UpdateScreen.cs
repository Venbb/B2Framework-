using B2Framework;
using UnityEngine;
using UnityEngine.UI;

namespace B2Framework
{
    public class UpdateScreen : MonoBehaviour, IUpdater
    {
        public Text version;
        public Slider progressBar;
        public Text progressText;

        private void Start()
        {
            version.text = string.Format(GameConst.VERSION, "0.0.0.0", "0.0.0.0");
            var updater = FindObjectOfType<Updater>();
            updater.listener = this;
        }

        #region IUpdateManager implementation
        public void OnStart()
        {

        }

        public void OnMessage(string msg)
        {
            progressText.text = msg;
        }

        public void OnProgress(float progress)
        {
            progressBar.value = progress;
        }

        public void OnVersion(string ver)
        {
            version.text = string.Format(GameConst.VERSION, ver, ver);;
        }
        public void OnClear()
        {

        }
        #endregion
    }
}