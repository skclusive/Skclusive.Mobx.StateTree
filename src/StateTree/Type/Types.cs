using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public static class Types
    {
        /**
         * Creates a type that can only contain a string value.
         * This type is used for string values by default
         *
         * @export
         * @alias types.string
         * @example
         * const Person = types.model({
         *   firstName: types.string,
         *   lastName: "Doe"
         * })
         */
        public readonly static ISimpleType<string> String = new StringType();


        /**
         * Creates a type that can only contain a numeric value.
         * This type is used for numeric values by default
         *
         * @export
         * @alias types.number
         * @example
         * const Vector = types.model({
         *   x: types.number,
         *   y: 0
         * })
         */
        public readonly static ISimpleType<decimal> Decimal = new DecimalType();

        public readonly static ISimpleType<double> Double = new DoubleType();

        public readonly static ISimpleType<int> Int = new IntType();

        /**
         * Creates a type that can only contain a boolean value.
         * This type is used for boolean values by default
         *
         * @export
         * @alias types.boolean
         * @example
         * const Thing = types.model({
         *   isCool: types.boolean,
         *   isAwesome: false
         * })
         */
        public readonly static ISimpleType<bool> Boolean = new BooleanType();

        /**
         * The type of the value `null`
         *
         * @export
         * @alias types.null
         */
        public readonly static ISimpleType<object> Null = new NullType();

        public readonly static ISimpleType<object> Frozen = new FrozenType<object>();

        /**
         * Identifiers are used to make references, lifecycle events and reconciling works.
         * Inside a state tree, for each type can exist only one instance for each given identifier.
         *
         * @example
         *  const Todo = types.model("Todo", {
         *      id: types.identifier,
         *      title: types.string
         *  })
         *
         * @export
         * @alias types.identifier
         * @template T
         * @param {IType<T, T>} baseType
         * @returns {IType<T, T>}
         */
        public readonly static IType<string, string> Identifier = new StringIdentifierType();

        /**
        * Similar to `types.identifier`, but `identifierNumber` will serialize from / to a number when applying snapshots
        *
        * @example
        *  const Todo = types.model("Todo", {
        *      id: types.identifierNumber,
        *      title: types.string
        *  })
        *
        * @export
        * @alias types.identifierNumber
        * @template T
        * @returns {IType<T, T>}
        */
        public readonly static IType<int, int> IdentifierInt = new IntIdentifierType();

        public static ISimpleType<object> GetPrimitiveFactoryFromValue(object value)
        {
            switch (value.GetType().Name.ToLower())
            {
                case "string":
                    return String as ISimpleType<object>;
                case "double":
                    return Double as ISimpleType<object>;
                case "int":
                    return Int as ISimpleType<object>;
                case "decimal":
                case "number":
                    return Decimal as ISimpleType<object>;
                case "boolean":
                    return Boolean as ISimpleType<object>;
                    //case "object":
                    //            if (value instanceof Date) return DatePrimitive
            }

            throw new Exception("Cannot determine primitive type from value " + value);
        }

        public static bool IsType(object type)
        {
            return type is IType;
        }

        public static bool IsPrimitiveType(object type)
        {
            return (
                IsType(type) &&
                ((int)((type as IType).Flags & (TypeFlags.String | TypeFlags.Number | TypeFlags.Boolean | TypeFlags.Date)) >
                    0));
        }

        public static IType<S, T> Optional<S, T>(IType<S, T> type, S defaultValue)
        {
            return Optional(type, () => defaultValue);
        }

        public static IType<S, T> Optional<S, T>(IType<S, T> type, Func<S> defaultValue)
        {
            return new OptionalType<S, T>(type, defaultValue);
        }

        public static IType<S, T> Union<S, T>(params IType[] types)
        {
            return Union<S, T>(null, types);
        }

        public static IType<S, T> Union<S, T>(UnionOptions options, params IType[] types)
        {
            var name = $"({string.Join(" | ", types.Select(type => type.Name))})";

            return new UnionType<S, T>(name, types, options);
        }

        public static IType<object, object> OptionalNull = Optional(Types.Null, null);

        public static IType<S, T> Maybe<S, T>(IType<S, T> type)
        {
            return Union<S, T>(OptionalNull, type);
        }

        public static ISimpleType<T> Literal<T>(T value)
        {
            if (!(value is string || value.GetType().IsPrimitive || value.GetType().IsEnum))
            {
                throw new InvalidOperationException("Literal types can be built only on top of primitives");
            }

            return new LiteralType<T>(value);
        }

        public static IType<S, T> Late<S, T>(string name, Func<IType<S, T>> definition)
        {
            return new LateType<S, T>(name, definition);
        }

        public static IObjectType<S, T> Late<S, T>(string name, Func<IObjectType<S, T>> definition)
        {
            return new LateObjectType<S, T>(name, definition);
        }

        public static IType<T, T> Enumeration<T>(params T[] enums)
        {
            var types = enums.Select(enumx => Literal<T>(enumx)).ToArray();

            return Union<T, T>(types);
        }

        public static IType<S, T> Custom<S, T>(ICustomTypeOptions<S, T> options)
        {
            return new CustomType<S, T>(options);
        }

        //public static IType<int, T> Reference<T>(IType<int, T> targetType)
        //{
        //    return new IdentifierReferenceType<int, T>(targetType);
        //}

        public static IType<string, T> Reference<S, T>(IType<S, T> targetType)
        {
            return new IdentifierReferenceType<string, S, T>(targetType);
        }

        public static IType<I, T> Reference<I, S, T>(IType<S, T> targetType)
        {
            return new IdentifierReferenceType<I, S, T>(targetType);
        }

        public static IType<int, T> Reference<T>(IType<int, T> targetType, IReferenceOptions<int, T> options)
        {
            return new CustomReferenceType<int, T>(targetType, options);
        }

        public static IType<string, T> Reference<T>(IType<string, T> targetType, IReferenceOptions<string, T> options)
        {
            return new CustomReferenceType<string, T>(targetType, options);
        }

        public static IType<S, T> Refinement<S, T>(string name, IType<S, T> type, Func<S, bool> predicator, Func<S, string> validator)
        {
            return new RefinementType<S, T>(name, type, predicator, validator);
        }

        public static IType<S[], IObservableList<INode, T>> List<S, T>(IType<S, T> subtype)
        {
            return new ListType<S, T>($"{subtype.Name}[]", subtype);
        }

        public static IType<IMap<string, S>, IObservableMap<string, INode, T>> Map<S, T>(IType<S, T> subtype)
        {
            return new MapType<S, T>($"Map<string, {subtype.Name}>", subtype);
        }

        public static IObjectType<S, T> Object<S, T>(string name) where S : class
        {
            return new ObjectType<S, T>(name);
        }

        public static IObjectType<S, T> Object<S, T>(IObjectTypeConfig<S, T> config) where S : class
        {
            return new ObjectType<S, T>(config);
        }

        public static IObjectType<T> Object<T>(string name)
        {
            return new ObjectType<T>(name);
        }

        public static IObjectType<T> Object<T>(IObjectTypeConfig<IDictionary<string, object>, T> config)
        {
            return new ObjectType<T>(config);
        }

        public static IObjectType<S, T> Compose<S, T, S1, T1>(string name, IObjectType<S1, T1> type1) where S : S1 where T : T1
        {
            return new ObjectType<S, T>(name).Include(type1);
        }

        public static IObjectType<S, T> Compose<S, T, S1, T1, S2, T2>(string name, IObjectType<S1, T1> type1, IObjectType<S2, T2> type2) where S : S1, S2 where T : T1, T2
        {
            return new ObjectType<S, T>(name).Include(type1).Include(type2);
        }

        public static IObjectType<S, T> Compose<S, T, S1, T1, S2, T2, S3, T3>(string name, IObjectType<S1, T1> type1, IObjectType<S2, T2> type2, IObjectType<S3, T3> type3) where S : S1, S2, S3 where T : T1, T2, T3
        {
            return new ObjectType<S, T>(name).Include(type1).Include(type2).Include(type3);
        }

        public static bool IsReferenceType(IType type)
        {
            return (type.Flags & TypeFlags.Reference) > 0;
        }

        public static bool IsFrozenType(IType type)
        {
            return (type.Flags & TypeFlags.Frozen) > 0;
        }

        public static bool IsIdentifierType(IType type)
        {
            return (type.Flags & TypeFlags.Identifier) > 0;
        }

        public static bool IsLateType(IType type)
        {
            return (type.Flags & TypeFlags.Late) > 0;
        }

        public static bool IsLiteralType(IType type)
        {
            return (type.Flags & TypeFlags.Literal) > 0;
        }

        public static bool IsOptionalType(IType type)
        {
            return (type.Flags & TypeFlags.Optional) > 0;
        }

        public static bool IsUnionType(IType type)
        {
            return (type.Flags & TypeFlags.Union) > 0;
        }

        public static bool IsRefinementType(IType type)
        {
            return (type.Flags & TypeFlags.Refinement) > 0;
        }

        public static bool IsListType(IType type)
        {
            return (type.Flags & TypeFlags.List) > 0;
        }

        public static bool IsMapType(IType type)
        {
            return (type.Flags & TypeFlags.Map) > 0;
        }

        public static bool IsObjectType(IType type)
        {
            return (type.Flags & TypeFlags.Object) > 0;
        }
    }
}
