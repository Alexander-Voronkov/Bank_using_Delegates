using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace delegates
{
    struct Owner
    {
        public string name;
        public string lastname;
        public string patronymic;
        public Owner(string n, string l, string p)
        {
            name = n;
            lastname = l;
            patronymic = p;
        }
        public override string ToString()
        {
            return name + " " + lastname + " " + patronymic;
        }
    }
    abstract class CardChanges : EventArgs
    {
        public string info { get; set; }
        public CardChanges(string change) { info = change; }
        public CardChanges(){ info = null; }
        class ReinforcementNotification : CardChanges
        {
            public ReinforcementNotification(string change = "Поповнення рахунку") : base(change) { }
        }
        class Withdrawal : CardChanges
        {
            public Withdrawal(string change = "Витрата коштів з рахунку") : base(change) { }
        }
        class PINChanges : CardChanges
        {
            public PINChanges(string change = "Зміна ПІН-коду") : base(change) { }
        }
        class CreditStart : CardChanges
        {
            public CreditStart(string change = "Початок використання кредитних коштів") : base(change) { }
        }
        class CreditEnd : CardChanges
        {
            public CreditEnd(string change = "Вичерпання кредитного ліміту") : base(change) { }
        }
        class CreditCard
        {
            public event EventHandler<CardChanges> Reinforcement;
            public event EventHandler<CardChanges> Withdrawal;
            public event EventHandler<CardChanges> PINChanges;
            public event EventHandler<CardChanges> CreditStart;
            public event EventHandler<CardChanges> CreditEnd;
            string Number;
            Owner Owner;
            DateTime Expiration;
            string PIN;
            int Balance = 0;
            int Credit = 0;
            int CreditLimit = 0;
            public override string ToString()
            {
                return $"Number:{Number}\nOwner:{Owner}\nExpiration Date:{Expiration}\nPin:{PIN}\nBalance:{Balance}\nCredit:{Credit}\nCreditLimit:{CreditLimit}\n";
            }
            public void ChangeCreditLimit(int newcl)
            {
                this.CreditLimit = newcl;

            }
            public CreditCard(string number, string name, string lastname, string patronymic, string PIN)
            {
                if (number.Length != 16)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Number += (new Random().Next(1000, 9999)).ToString();
                    }
                }
                else this.Number = number;
                this.Owner = new Owner(name, lastname, patronymic);
                this.Expiration = DateTime.Now.AddYears(5);                
                if((new Regex(@"\d{4}")).IsMatch(PIN))
                    this.PIN = PIN;
                else 
                    this.PIN = (new Random()).Next(1000, 9999).ToString();
            }
            public static CreditCard operator +(CreditCard c, int amount)
            {
                CreditCard q = new CreditCard(c.Number, c.Owner.name, c.Owner.lastname, c.Owner.patronymic, c.PIN);
                q.Balance += amount;
                q.Reinforcement = Informer.SendMessage;
                q.Reinforcement?.Invoke(q, new ReinforcementNotification($"Відбулося поповнення рахунку на {amount}"));
                q.Reinforcement -= Informer.SendMessage;
                return q;
            }
            public static CreditCard operator -(CreditCard c, int amount)
            {
                CreditCard q = c;
                if (q.Balance - amount >= 0)
                {
                    q.Balance -= amount;
                    q.Withdrawal += Informer.SendMessage;
                    q.Withdrawal?.Invoke(q, new Withdrawal());
                    q.Withdrawal -= Informer.SendMessage;
                }
                else if (q.Balance == 0 && q.CreditLimit >= amount)
                {
                    q.CreditStart += Informer.SendMessage;
                    q.CreditStart?.Invoke(q, new CreditStart());
                    q.CreditStart -= Informer.SendMessage;
                    q.Credit -= amount;
                }
                else if (q.Balance == 0 && -q.CreditLimit >= q.Credit - amount)
                {
                    q.CreditEnd += Informer.SendMessage;
                    q.CreditEnd?.Invoke(q, new CreditEnd());
                    q.CreditEnd-= Informer.SendMessage;
                }
                return q;
            }
            public void ChangePIN(string PIN)
            {
                Regex regex = new Regex(@"\d{4}");
                if(PIN.Length==4&&regex.IsMatch(PIN))
                this.PIN = PIN;
                this.PINChanges += Informer.SendMessage;
                this.PINChanges?.Invoke(this, new PINChanges());
                this.PINChanges-=Informer.SendMessage;
            }

        }
        static class Informer
        {
            public static void SendMessage(object sender, CardChanges cc)
            {
                Console.WriteLine(cc.info);
            }
        }
        internal class Program
        {
            static void Main(string[] args)
            {
                CreditCard cc = new CreditCard("5168114434211123","Olexandr","Voronkov","Serhijovich","wggh");
                Console.WriteLine(cc);
                cc.ChangePIN("1234");
                Console.WriteLine(cc);
                cc.ChangeCreditLimit(50000);
                cc -= 20000;
                Console.WriteLine(cc);
                Console.Read();
            }
        }
    }
}
