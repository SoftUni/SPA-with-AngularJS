app.controller('AdsController', function AdsController($scope, adsData) {
	adsData.getAds(
		function (data, status, headers, config) {
			$scope.data = data;
		}, 
		function (error, status, headers, config) {
			console.log(status, error); // Try with invalid url
		});
})