using System;

namespace RaceManager.Root
{
    public interface ISaveable
    {
        Type DataType();
        void Load(object data);
        object Save();
    }
}

