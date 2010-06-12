using Castle.Core;

namespace Castle.MicroKernel.Proxy
{
    /// <summary>
    /// Select the appropriate interecptors based on the application specific
    /// business logic
    /// </summary>
    public interface IModelInterceptorsSelector
    {
        /// <summary>
        /// Select the appropriate intereceptor references.
        /// The intereceptor references aren't neccessarily registered in the model.Intereceptors
        /// </summary>
        /// <param name="model">The model to select the interceptors for</param>
        /// <param name="interceptors">The interceptors selected by previous selectors in the pipeline or <see cref="ComponentModel.Interceptors"/> if this is the first interceptor in the pipeline.</param>
        /// <returns>The intereceptors for this model (in the current context) or a null reference</returns>
        /// <remarks>
        /// If the selector is not interested in modifying the interceptors for this model, it 
        /// should return <paramref name="interceptors"/> and the next selector in line would be executed.
        /// If the selector wants no interceptors to be used it can either return <c>null</c> or empty array.
        /// However next interceptor in line is free to override this choice.
        /// </remarks>
        InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors);

        /// <summary>
        /// Determain whatever the specified has interecptors.
        /// The selector should only return true from this method if it has determained that is
        /// a model that it would likely add interceptors to.
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>Whatever this selector is likely to add intereceptors to the specified model</returns>
        bool HasInterceptors(ComponentModel model);
    }
}