using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Generic;
#if USE_ADDRESSABLES
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

namespace PolytopeSolutions.Toolset.AssetManagement {
    public class AddressableHelpers {
        #if USE_ADDRESSABLES
        [Serializable]
        public class AssetReferenceMaterial : AssetReferenceT<Material> {
            public AssetReferenceMaterial(string guid) : base(guid) {}
        }

        public interface IOperation {
            public AssetReference reference { get; set; }
            public void Release();
        }
        public abstract class Operation<T> : IOperation {
            public AssetReference reference { get; set; }
            private AsyncOperationHandle handle { get; set; }
            public bool IsDone => this.handle.IsDone;


            protected abstract void Invoke(object result);

            protected Operation(AssetReference reference) {
                this.reference = reference;
            }

            protected void Init() {
                this.handle = this.reference.LoadAssetAsync<T>();
                this.handle.Completed += HandleCompleted;
            }

            private void HandleCompleted(AsyncOperationHandle handle) {
                if (handle.Status == AsyncOperationStatus.Succeeded) {
                    Invoke(handle.Result);
                }
                else {
                    this.LogError($"AssetReference {this.reference.RuntimeKey} failed to load.");
                }
            }

            public void Release() {
                this.reference.ReleaseAsset();
            }
        }
        public class ByteOperation : Operation<TextAsset> {
            private Action<byte[]> callback;

            public ByteOperation(AssetReference _reference, Action<byte[]> _callback) : base(_reference) {
                this.callback = _callback;
                Init();
            }
            protected override void Invoke(object result) => Invoke(this.callback, result);
            public static void Invoke(Action<byte[]> callback, object result) {
                TextAsset asset = result as TextAsset;
                callback.Invoke(asset.bytes);
            }
        }
        public class TextOperation : Operation<TextAsset> {
            private Action<string> callback;

            public TextOperation(AssetReference _reference, Action<string> _callback) : base(_reference) {
                this.callback = _callback;
                Init();
            }
            protected override void Invoke(object result) => Invoke(this.callback, result);
            public static void Invoke(Action<string> callback, object result) {
                TextAsset asset = result as TextAsset;
                callback.Invoke(asset.text);
            }
        }
        public class SpriteOperation : Operation<Sprite> {
            private GameObject goHolder;
            private Action<Sprite> callback;

            public SpriteOperation(AssetReference _reference, GameObject _goHolder, Action<Sprite> _callback) : base(_reference) {
                if (_goHolder)
                    this.goHolder = _goHolder;
                if (_callback != null)
                    this.callback = _callback;
                Init();
            }
            protected override void Invoke(object result) => Invoke(this.goHolder, this.callback, result);
            public static void Invoke(GameObject goHolder, Action<Sprite> callback, object result) {
                Sprite asset = result as Sprite;
                if (goHolder)
                    goHolder.SetImage(asset);
                if (callback != null)
                    callback.Invoke(asset);
            }
        }
        public class ScriptableObjectOperation<T> : Operation<ScriptableObject> where T : ScriptableObject {
            private Action<T> callback;

            public ScriptableObjectOperation(AssetReference _reference, Action<T> _callback) : base(_reference) {
                this.callback = _callback;
                Init();
            }
            protected override void Invoke(object result) => Invoke(this.callback, result);
            public static void Invoke(Action<T> callback, object result) {
                T asset = result as T;
                callback.Invoke(asset);
            }
        }
        public class GameObjectOperation : Operation<GameObject> {
            private Action<GameObject> callback;

            public GameObjectOperation(AssetReference _reference, Action<GameObject> _callback) : base(_reference) {
                this.callback = _callback;
                Init();
            }
            protected override void Invoke(object result) => Invoke(this.callback, result);
            public static void Invoke(Action<GameObject> callback, object result) {
                GameObject asset = result as GameObject;
                callback.Invoke(asset);
            }
        }
        public class MaterialOperation : Operation<Material> {
            private Action<Material> callback;

            public MaterialOperation(AssetReference _reference, Action<Material> _callback) : base(_reference) {
                this.callback = _callback;
                Init();
            }
            protected override void Invoke(object result) => Invoke(this.callback, result);
            public static void Invoke(Action<Material> callback, object result) {
                Material asset = result as Material;
                callback.Invoke(asset);
            }
        }
        #endif
    }
}