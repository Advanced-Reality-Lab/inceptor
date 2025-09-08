using System.Collections;
using UnityEngine.Events;

namespace InceptorEngine.Analysis.Interfaces
{
    public interface ITranslateProvider
    {
        public struct TranslationData
        {
            public string Text;
            public UnityAction<string> TranslationCallback;
        }
        
        IEnumerator TranslateFromHebrewToEnglish(TranslationData translationData);

        void CallTranslateCoroutine(string text, UnityAction<string> translationCallback);
    }
}
