using Nua.Types;

namespace Nua
{
    public class NuaContext
    {
        public Frame GlobalFrame = new();
        public Stack<Frame> Frames = new();

        public void SetGlobal(string name, NuaValue? value)
        {
            if (value is not null)
                GlobalFrame.Variables[name] = value;
            else
                GlobalFrame.Variables.Remove(name);
        }

        public NuaValue? GetGlobal(string name)
        {
            if (GlobalFrame.Variables.TryGetValue(name, out var globalValue))
                return globalValue;
            else
                return null;
        }

        public void Set(string name, NuaValue? value)
        {
            Frame targetFrame;

            if (Frames.TryPeek(out var frame) && !frame.GlobalTags.Contains(name))
                targetFrame = frame;
            else
                targetFrame = GlobalFrame;

            if (value is not null)
                targetFrame.Variables[name] = value;
            else
                targetFrame.Variables.Remove(name);
        }

        public NuaValue? Get(string name)
        {
            if (Frames.TryPeek(out var frame) && frame.Variables.TryGetValue(name, out var valueInFrame))
            {
                return valueInFrame;
            }
            else if (GlobalFrame.Variables.TryGetValue(name, out var globalValue))
            {
                return globalValue;
            }

            return null;
        }

        public void PushFrame() => Frames.Push(new());
        public void PopFrame() => Frames.Pop();
        public void TagGlobal(string name)
        {
            if (Frames.TryPeek(out var frame))
                frame.GlobalTags.Add(name);
        }

        public class Frame
        {
            public readonly Dictionary<string, NuaValue> Variables = new();
            public readonly HashSet<string> GlobalTags = new();
        }
    }
}
