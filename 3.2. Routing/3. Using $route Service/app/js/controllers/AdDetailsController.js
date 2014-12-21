app.controller('AdDetailsController', 
	function ($scope, $routeParams, $route, adsData) {
		$scope.ad = adsData.getById($routeParams.adId);

		// http://localhost:1111/#/ads/77?page=3
		console.log($route.current.test);
		console.log($route.current.params.page); // 3
		console.log($route.current.params.adId); // 77
		console.log($route.current.pathParams.adId); // 77
		console.log($route.current.pathParams.page); // undefined
	}
);