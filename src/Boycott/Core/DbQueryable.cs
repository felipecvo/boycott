namespace Boycott.Core {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections;
    using System.Linq.Expressions;
    using Boycott.Provider;

    public class DbQueryable<T> : IOrderedQueryable<T> {
        private DatabaseProvider provider;
        private Expression expression;

        public DbQueryable() {
            this.provider = Configuration.DatabaseProvider;
            this.expression = Expression.Constant(this);
        }

        public void SetExpression(Expression expression) {
            this.expression = expression;
        }

        public IEnumerator<T> GetEnumerator() {
            return this.provider.Execute<IEnumerable<T>>(this.expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.provider.Execute<IEnumerable<T>>(this.expression).GetEnumerator();
        }

        public Type ElementType {
            get { return typeof(T); }
        }

        public Expression Expression {
            get { return this.expression; }
        }

        public IQueryProvider Provider {
            get { return this.provider; }
        }
    }
}
