using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class SceneStack
    {
        private List<Scene> scenes = new List<Scene>();
        private List<Operation> nextOperations = new List<Operation>();

        // scenesに対する1つの操作
        private struct Operation
        {
            public enum Kind
            {
                Push, Pop
            }

            public Scene scene;
            public Kind action;

            public static Operation CreatePopOperation()
            {
                Operation o = new Operation();
                o.scene = null;
                o.action = Kind.Pop;
                return o;
            }

            public static Operation CreatePushOperation(Scene s)
            {
                Operation o = new Operation();
                o.scene = s;
                o.action = Kind.Push;
                return o;
            }
        }

        public Scene CurrentScene
        {
            get
            {
                return scenes[scenes.Count - 1];
            }

            private set
            {
                scenes[scenes.Count - 1] = value;
            }
        }

        public SceneStack()
        {
            // 番兵を追加しておく
            Scene s = new Scene();
            s.Init(null);
            s.SetStack(this);
            scenes.Add(s);
        }

        public void Push(Scene s)
        {
            nextOperations.Add(Operation.CreatePushOperation(s));
        }

        public void Pop()
        {
            nextOperations.Add(Operation.CreatePopOperation());
        }

        public void ChangeCurrentScene(Scene newScene)
        {
            nextOperations.Add(Operation.CreatePopOperation());
            nextOperations.Add(Operation.CreatePushOperation(newScene));
        }

        public void Update(GameStatus status)
        {
            foreach (var op in nextOperations)
            {
                switch (op.action)
                {
                    case Operation.Kind.Pop:
                        if (scenes.Count >= 2)
                        {
                            // 現在のシーンをまずポーズさせて、次にスタックとの接続を解除する
                            CurrentScene.Pause();
                            CurrentScene.SetStack(null);

                            // スタックから削除する
                            scenes.RemoveAt(scenes.Count - 1);

                            // 新しく一番上になったシーンを再開させる
                            CurrentScene.Resume();
                        }
                        break;
                    case Operation.Kind.Push:
                        // 現在のシーンをポーズさせる
                        CurrentScene.Pause();

                        // まずプッシュするシーンとスタックを連結させ、次にスタックに追加する
                        op.scene.SetStack(this);
                        scenes.Add(op.scene);

                        // 新しく一番上になったシーンを開始させる
                        CurrentScene.Resume();
                        break;
                }
            }
            nextOperations.Clear();

            foreach (var s in scenes)
            {
                s.Update(status);
            }
        }

        public void Draw()
        {
            foreach (var s in scenes)
            {
                s.Draw();
            }
        }
    }
}
