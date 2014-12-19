app.controller('CacheExampleController', 
	function CacheExampleController($scope, appCache) {
		$scope.addToCache = function (key, value) {
			appCache.put(key, value);
		};

		$scope.readFromCache = function (key) {
			return appCache.get(key);
		}
});