#pragma once
#include <concepts>

template <class T>
concept is_test = requires (T & x) {
	{x.initialize()} -> std::convertible_to<bool>;
	{x.run()} -> std::convertible_to<void>;
	{x.shutdown()} -> std::convertible_to<void>;
};

template <is_test T>
class test_runner
{
private:
	T t;
public:

	bool initialize()
	{
		return t.initialize();
	}

	void run()
	{
		t.run();
	}

	void shutdown()
	{
		t.shutdown();
	}
};