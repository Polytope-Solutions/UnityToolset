
using System.Collections.Generic;
using UnityEngine;

using System;

using PolytopeSolutions.Toolset.GlobalTools.Types;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using static PolytopeSolutions.Toolset.AssetManagement.AddressableHelpers;
#endif

namespace PolytopeSolutions.Toolset.AssetManagement {
    public class AddressableManager : TManager<AddressableManager> {
        #if USE_ADDRESSABLES
        private List<IOperation> currentOperations;
        private HashSet<AssetReference> currentAssetReferences;

        protected override void Awake() {
            base.Awake();
            this.currentOperations = new List<IOperation>();
            this.currentAssetReferences = new HashSet<AssetReference>();
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            foreach (IOperation operation in this.currentOperations) {
                this.currentAssetReferences.Remove(operation.reference);
                operation.Release();
            }
        }

        public void RequestAddressableBytes(AssetReference assetReference, Action<byte[]> callback) {
            if (this.currentAssetReferences.Contains(assetReference)) {
                ByteOperation.Invoke(callback, assetReference.Asset);
            }
            else {
                IOperation operation = new ByteOperation(assetReference, callback);
                this.currentOperations.Add(operation);
                this.currentAssetReferences.Add(assetReference);
            }
        }
        public void RequestAddressableText(AssetReference assetReference, Action<string> callback) {
            if (this.currentAssetReferences.Contains(assetReference)) {
                TextOperation.Invoke(callback, assetReference.Asset);
            }
            else {
                IOperation operation = new TextOperation(assetReference, callback);
                this.currentOperations.Add(operation);
                this.currentAssetReferences.Add(assetReference);
            }
        }
        public void RequestAddressableSprite(AssetReference assetReference, GameObject goHolder) {
            if (this.currentAssetReferences.Contains(assetReference)) {
                SpriteOperation.Invoke(goHolder, null, assetReference.Asset);
            }
            else {
                IOperation operation = new SpriteOperation(assetReference, goHolder, null);
                this.currentOperations.Add(operation);
                this.currentAssetReferences.Add(assetReference);
            }
        }
        public void RequestAddressableSprite(AssetReference assetReference, Action<Sprite> callback) {
            if (this.currentAssetReferences.Contains(assetReference)) {
                SpriteOperation.Invoke(null, callback, assetReference.Asset);
            }
            else {
                IOperation operation = new SpriteOperation(assetReference, null, callback);
                this.currentOperations.Add(operation);
                this.currentAssetReferences.Add(assetReference);
            }
        }
        public void RequestAddressableScriptableObject<T>(AssetReference assetReference, Action<T> callback) where T : ScriptableObject {
            if (this.currentAssetReferences.Contains(assetReference)) {
                ScriptableObjectOperation<T>.Invoke(callback, assetReference.Asset);
            }
            else {
                IOperation operation = new ScriptableObjectOperation<T>(assetReference, callback);
                this.currentOperations.Add(operation);
                this.currentAssetReferences.Add(assetReference);
            }
        }
        public void RequestAddressableGameObject(AssetReference assetReference, Action<GameObject> callback) {
            if (this.currentAssetReferences.Contains(assetReference)) {
                GameObjectOperation.Invoke(callback, assetReference.Asset);
            }
            else {
                IOperation operation = new GameObjectOperation(assetReference, callback);
                this.currentOperations.Add(operation);
                this.currentAssetReferences.Add(assetReference);
            }
        }
        public void RequestAddressableMaterial(AssetReference assetReference, Action<Material> callback) {
            if (this.currentAssetReferences.Contains(assetReference)) {
                MaterialOperation.Invoke(callback, assetReference.Asset);
            }
            else {
                IOperation operation = new MaterialOperation(assetReference, callback);
                this.currentOperations.Add(operation);
                this.currentAssetReferences.Add(assetReference);
            }
        }
        #endif
    }
}