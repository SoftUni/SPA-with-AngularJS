app.controller('AdDetailsController', 
	function ($scope, $routeParams, adsData) {
		$scope.ad = adsData.getById($routeParams.adId);
});