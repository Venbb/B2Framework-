using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public interface IUIState
    {
        bool isLoading { get; }
        string key { get; }
        System.Type uiType { get; }
        GameObject parent { get; }
        UIRootComponent uiRoot { get; }
        System.Collections.ObjectModel.ReadOnlyCollection<UIPrefab> uiPrefabs { get; }
        UIPrefab firstUIPrefab { get; }
        UIEvent uiEvent { get; }

        void SetLoading(bool value);
        bool IsValid(bool isPrintError = true);
        void RemovePrefabAtIndex(int index);
    }
}
