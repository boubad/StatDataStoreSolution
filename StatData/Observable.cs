using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
//
namespace StatData
{
    public class Observable : INotifyPropertyChanged
    {
        public Observable()
        {
        }
        public void NotifyChange(String propertyName)
        {
            this.NotifyPropertyChanged(propertyName);
        }
        public void NotifyChanges(IEnumerable<String> names)
        {
            foreach (var s in names)
            {
                NotifyPropertyChanged(s);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var pp = this.PropertyChanged;
            if (pp != null)
            {
                var p = this;
                pp(p, new PropertyChangedEventArgs(propertyName));
            }
        }//NotifyPropertyChanged
        protected void NotifyPropertyChanged(Expression<Func<object>> expression)
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }
            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo != null)
                    NotifyPropertyChanged(propertyInfo.Name);
            }
        }
    }// public class Observable
}
