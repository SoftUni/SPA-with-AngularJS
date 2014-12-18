app.controller('NewsController', 
	function NewsController($scope, newsData) {
		$scope.sortOrder = 'author';
		$scope.news = newsData.news;

		$scope.upVote = function upVote(comment) {
			comment.votes += 1;
		}

		$scope.downVote = function downVote(comment) {
			comment.votes -= 1;
		}
	});