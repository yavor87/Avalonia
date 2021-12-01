using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;

namespace Avalonia
{
    public static class Direct2DApplicationExtensions
    {
        public static T UseIos<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            return builder.UseRenderingSubsystem(() =>
                {
                    Skia.SkiaPlatform.Initialize(AvaloniaLocator.Current.GetService<SkiaOptions>() ??
                                                 new SkiaOptions());
                    AvaloniaLocator.CurrentMutable.Bind<ITextShaperImpl>().ToConstant(new TextShaperImpl());
                },
                "iOS");
        }
    }

    public class AppBuilder : AppBuilderBase<AppBuilder>
    {
        public AppBuilder() : base(new StandardRuntimePlatform(),
            b => StandardRuntimePlatformServices.Register(b.ApplicationType.Assembly))
        {
            this.UseIos().UseWindowingSubsystem(iOS.Platform.Register);
        }
    }
}
