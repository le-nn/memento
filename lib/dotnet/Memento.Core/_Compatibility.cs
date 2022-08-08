namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct |
                    AttributeTargets.Field |
                    AttributeTargets.Property,
                    AllowMultiple = false,
                    Inherited = false)]
    public sealed class RequiredMemberAttribute : Attribute {
        public RequiredMemberAttribute() { }
    }
}

namespace System.Diagnostics.CodeAnalysis {
    [AttributeUsage(AttributeTargets.Constructor,
                    AllowMultiple = false,
                    Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute {
        public SetsRequiredMembersAttribute() { }
    }
}

namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute {
        public CompilerFeatureRequiredAttribute(string featureName) {
            FeatureName = featureName;
        }
        public string FeatureName { get; }
        public string? Language { get; init; }
    }
}
