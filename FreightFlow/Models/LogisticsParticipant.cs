namespace FreightFlow.Models
{
    public abstract class LogisticsParticipant
    {
        private decimal totalEarned;

        public string Name { get; }
        public string Phone { get; }

        protected LogisticsParticipant(string name, string phone)
        {
            Name = name;
            Phone = phone;
        }

        public decimal TotalEarned
        {
            get { return totalEarned; }
        }

        protected void AddEarnings(decimal amount)
        {
            if (amount > 0)
                totalEarned += amount;
        }

        public abstract string Role { get; }

        public abstract decimal CalculateCommission(Load load);

        public virtual string GetInfo()
        {
            return $"{Role}: {Name}, tel. {Phone}, zarobleno ${TotalEarned:F2}";
        }
    }
}
