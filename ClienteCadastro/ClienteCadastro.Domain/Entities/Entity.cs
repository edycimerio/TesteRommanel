using System;

namespace ClienteCadastro.Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime DataCriacao { get; protected set; }
        public DateTime? DataAtualizacao { get; protected set; }
        public int Version { get; protected set; } = 1;

        protected Entity()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
        }

        public void AtualizarDataModificacao()
        {
            DataAtualizacao = DateTime.Now;
            Version++;
        }

        public override bool Equals(object? obj)
        {
            var compareTo = obj as Entity;

            if (ReferenceEquals(this, compareTo)) return true;
            if (ReferenceEquals(null, compareTo)) return false;

            return Id.Equals(compareTo.Id);
        }

        public static bool operator ==(Entity? a, Entity? b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity? a, Entity? b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 907) + Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}]";
        }
    }
}
