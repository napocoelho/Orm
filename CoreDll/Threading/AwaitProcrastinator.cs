using System.Threading.Tasks;

namespace CoreDll.Threading
{
    /// <summary>
    /// Provoca um delay no await com a opção de prorrogação com cancelamento de chamadas anteriores, 
    /// possibilitando que somente a última chamada seja realmente chamda.
    /// Substitui a função de BindingDelay de um TextoBox, por exemplo.
    /// </summary>
    public class AwaitProcrastinator
    {
        private object LOCK = new object();

        private int? callId = null;
        private int? CallId { get { lock (LOCK) { return callId; } } set { lock (LOCK) { callId = value; } } }
        //private System.Action Action { get; set; }
        private int Miliseconds { get; set; }
        public bool IsTriggered { get => CallId.HasValue; }

        public AwaitProcrastinator(int miliseconds)
        {
            Miliseconds = miliseconds;
        }


        public void Cancel()
        {
            CallId = null;
        }

        /// <summary>
        /// Apenas a última chamada deverá ser executada. 
        /// O código mais comum para fazê-lo funcionar seria:
        ///         if(await proc.Procrastinate())
        ///             return; //--> exit the funcion
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Procrastinate()
        {
            CallId = (CallId ?? 0);
            int internalCallId = (++CallId).Value;

            await Task.Delay(Miliseconds).ConfigureAwait(true);

            bool procrastinate = CallId != internalCallId;

            if (!procrastinate)
                CallId = null;

            return procrastinate;
        }
    }
}
