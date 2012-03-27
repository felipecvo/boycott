namespace Boycott.Core {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Linq.Expressions;

    public interface ISelfQuery {
        void SetExpression(Expression expression);
    }
}
