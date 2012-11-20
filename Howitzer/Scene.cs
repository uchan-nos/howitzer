
#if DEBUG
#define CHECK_INITIALIZED
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class Scene : IDisposable
    {
        private SceneStack scenes = null;

        /// <summary>
        /// このシーンを指定されたシーンスタックに所属させる。
        /// SceneStackから呼び出されることを想定している。
        /// </summary>
        /// <param name="scenes">シーンスタック</param>
        public void SetStack(SceneStack scenes)
        {
            this.scenes = scenes;
        }


        public enum States
        {
            Resumed, Paused
        }

        public States Status
        {
            get;
            private set;
        }

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
        /// シーンを初期化する
        /// </summary>
        /// <param name="settings">ゲーム設定</param>
        public Scene Init(GameSettings settings)
        {
#if CHECK_INITIALIZED
            this.initialized = true;
#endif
            this.GameSettings = settings;
            this.Status = States.Paused;

            _Init(settings);

            return this;
        }

        protected virtual void _Init(GameSettings settings)
        {
        }

        /// <summary>
        /// シーンを更新する
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
        /// シーンを描画する
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
        /// シーンを破棄する
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

        /// <summary>
        /// シーンが最前面でなくなる
        /// </summary>
        public void Pause()
        {
#if CHECK_INITIALIZED
            if (!initialized)
            {
                throw new NotInitializedException();
            }
#endif

            this.Status = States.Paused;
            _Pause();
        }

        protected virtual void _Pause()
        {
        }

        /// <summary>
        /// シーンが最前面に復帰する
        /// </summary>
        public void Resume()
        {
#if CHECK_INITIALIZED
            if (!initialized)
            {
                throw new NotInitializedException();
            }
#endif

            this.Status = States.Resumed;
            _Resume();
        }

        protected virtual void _Resume()
        {
        }

        /// <summary>
        /// 次のシーンへ遷移する。現在のシーンをスタックに残す。
        /// </summary>
        /// <param name="s">遷移するシーン</param>
        protected void CallScene(Scene s)
        {
            if (scenes.CurrentScene == this)
            {
                scenes.Push(s);
            }
        }

        /// <summary>
        /// 次のシーンへ遷移する。現在のシーンをスタックから削除する。
        /// </summary>
        /// <param name="s">遷移するシーン</param>
        protected void GotoScene(Scene s)
        {
            if (scenes.CurrentScene == this)
            {
                scenes.ChangeCurrentScene(s);
            }
        }

        /// <summary>
        /// 現在のシーンを終了し、スタックから削除する。
        /// </summary>
        protected void EndScene()
        {
            if (scenes.CurrentScene == this)
            {
                scenes.Pop();
            }
        }
    }
}
