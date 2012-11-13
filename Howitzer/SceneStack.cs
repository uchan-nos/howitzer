using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class SceneStack
    {
        private List<Scene> scenes = new List<Scene>();

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
            CurrentScene.Stop();
            s.SetStack(this);
            scenes.Add(s);
            CurrentScene.Resume();
        }

        public void Pop()
        {
            if (scenes.Count >= 2)
            {
                CurrentScene.Stop();
                CurrentScene.SetStack(null);
                scenes.RemoveAt(scenes.Count - 1);
                CurrentScene.Resume();
            }
        }

        public void ChangeCurrentScene(Scene newScene)
        {
            CurrentScene.Stop();
            CurrentScene.SetStack(null);
            newScene.SetStack(this);
            CurrentScene = newScene;
            CurrentScene.Resume();
        }

        public void Update(GameStatus status)
        {
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
