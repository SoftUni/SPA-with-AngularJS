app.controller('AdDetailsController', function ($scope, $routeParams, $log, adsData) {
	adsData.getById($routeParams.adId)
		.$promise
		.then(function (data) {
			$scope.ad = data;
		}, function (error) {
			$log.error(error);
		});
});