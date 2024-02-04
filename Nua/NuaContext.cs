using Nua.Types;

namespace Nua
{
    public class NuaContext
    {
        private readonly Frame globalFrame = new();
        private readonly Stack<Frame> frames = new();

        public void SetGlobal(string name, NuaValue? value)
        {
            if (value is not null)
                globalFrame.Variables[name] = value;
            else
                globalFrame.Variables.Remove(name);
        }

        public NuaValue? GetGlobal(string name)
        {
            if (globalFrame.Variables.TryGetValue(name, out var globalValue))
                return globalValue;
            else
                return null;
        }

        public void Set(string name, NuaValue? value)
        {
            Frame targetFrame;

            if (frames.TryPeek(out var frame) && !frame.GlobalTags.Contains(name))
                targetFrame = frame;
            else
                targetFrame = globalFrame;

            if (value is not null)
                targetFrame.Variables[name] = value;
            else
                targetFrame.Variables.Remove(name);
        }

        public NuaValue? Get(string name)
        {
            if (frames.TryPeek(out var frame) && frame.Variables.TryGetValue(name, out var valueInFrame))
            {
                return valueInFrame;
            }
            else if (globalFrame.Variables.TryGetValue(name, out var globalValue))
            {
                return globalValue;
            }

            return null;
        }

        public void PushFrame() => frames.Push(new());
        public void PopFrame() => frames.Pop();
        public void TagGlobal(string name)
        {
            if (frames.TryPeek(out var frame))
                frame.GlobalTags.Add(name);
        }

        private class Frame
        {
            public readonly Dictionary<string, NuaValue> Variables = new();
            public readonly HashSet<string> GlobalTags = new();
        }
    }
}
