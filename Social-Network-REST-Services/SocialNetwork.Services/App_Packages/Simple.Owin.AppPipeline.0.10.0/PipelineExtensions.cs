using System;

namespace Simple.Owin.AppPipeline
{
    internal static class PipelineExtensions
    {
        public static IPipeline All(this IPipeline pipeline, Action<ControlComponent> setup) {
            var controler = new ControlComponent(ControlComponent.Match.All);
            setup(controler);
            pipeline.Use(controler);
            return pipeline;
        }

        public static IPipeline First(this IPipeline pipeline, Action<ControlComponent> setup) {
            var controler = new ControlComponent(ControlComponent.Match.First);
            setup(controler);
            pipeline.Use(controler);
            return pipeline;
        }
    }
}