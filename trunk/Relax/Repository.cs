﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relax.Design;

namespace Relax
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Get(string id);
        Document Save(TEntity entity);
        void Delete(TEntity entity);
    }

    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public Session Session { get; private set; }

        public IDictionary<string, Query<TEntity>> Queries { get; private set; }

        public Repository(Session sx) 
        {
            __init(sx, null);
        }

        public Repository(Session sx, DesignDocument design)
        {
            __init(sx, design);
        }

        private void __init(Session sx, DesignDocument design)
        {
            Queries = new Dictionary<string, Query<TEntity>>();
            Session = sx;

            if (null == design)
            {
                try
                {
                    // TODO: add special handling for session to cache this,
                    // will be very slow to ask every single time
                    design = Session.Load<DesignDocument>("_design/" + typeof (TEntity).Name.ToLowerInvariant());
                }
                catch 
                {
                    // its not a fault to not have a design document
                }
            }

            if (null != design)
            {
                foreach (var v in design.Views)
                {
                    Queries.Add(
                        v.Key,
                        new Query<TEntity>(
                            Session,
                            typeof(TEntity).Name.ToLowerInvariant(),
                            v.Key,
                            !String.IsNullOrEmpty(v.Value.Reduce)  
                    ));
                }
            }
        }

        public TEntity Get(string id)
        {
            return Session.Load<TEntity>(id);
        }

        public Document Save(TEntity entity)
        {
            return Session.Save(entity);
        }

        public void Delete(TEntity entity)
        {
            Session.Delete(entity);
        }
    }
}