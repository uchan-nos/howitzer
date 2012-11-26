
#if DEBUG
#define CHECK_INITIALIZED
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Howitzer
{
    class GameObject : IDisposable
    {
#if CHECK_INITIALIZED
        /// <summary>
        /// このインスタンスが初期化されているか
        /// </summary>
        private bool initialized = false;
#endif

        /// <summary>
        /// ゲーム設定（サブクラスはこのプロパティ経由でゲーム設定を参照する）
        /// </summary>
        public GameSettings GameSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// オブジェクトを初期化する
        /// </summary>
        /// <param name="settings">ゲーム設定</param>
        public void Init(GameSettings settings)
        {
#if CHECK_INITIALIZED
            this.initialized = true;
#endif
            this.GameSettings = settings;

            _Init(settings);
        }

        protected virtual void _Init(GameSettings settings)
        {
        }

        /// <summary>
        /// オブジェクトを更新する
        /// 1フレームに1回呼び出す
        /// </summary>
        /// <param name="status">更新時情報</param>
        public void Update(GameStatus status)
        {
#if CHECK_INITIALIZED
            if (!initialized)
            {
                throw new NotInitializedException();
            }
#endif

            _Update(status);
        }

        protected virtual void _Update(GameStatus status)
        {
        }

        /// <summary>
        /// オブジェクトを描画する
        /// </summary>
        public void Draw()
        {
#if CHECK_INITIALIZED
            if (!initialized)
            {
                throw new NotInitializedException();
            }
#endif

            _Draw();
        }

        protected virtual void _Draw()
        {
        }

        /// <summary>
        /// オブジェクトを破棄する
        /// </summary>
        public void Dispose()
        {
#if CHECK_INITIALIZED
            if (!initialized)
            {
                throw new NotInitializedException();
            }
#endif

            _Dispose();
        }

        protected virtual void _Dispose()
        {
        }
    }
}
