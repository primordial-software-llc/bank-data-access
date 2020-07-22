using System.Collections.Generic;

namespace Accounting
{
    public interface IJournal<T>
    {
        T Save(T entry);
        List<T> SendToAccounting();
    }
}
