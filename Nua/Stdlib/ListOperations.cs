using Nua.Types;

namespace Nua.Stdlib
{
    public class ListOperations : NuaNativeTable
    {
        private ListOperations() { }

        public static ListOperations Create()
        {
            return new ListOperations()
            {
                Storage =
                {
                    [new NuaString("add")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            var list = OperationsHelper.TakeListParam("add", parameters, 0);
                            OperationsHelper.EnsureParamInBound("add", parameters, 1);

                            list.Storage.Add(parameters[1]);
                            return list;
                        }),
                    [new NuaString("remove_at")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            var list = OperationsHelper.TakeListParam("remove_at", parameters, 0);
                            var index = (int)OperationsHelper.TakeNumberParam("remove_at", parameters, 1);

                            if (index >= 0 && index < list.Storage.Count)
                                list.Storage.RemoveAt(index);

                            return list;
                        }),
                }
            };
        }
    }
}
