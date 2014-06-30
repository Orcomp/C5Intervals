C5 Intervals
============


C5 Intervals assume that all intervals used are valid.
That is, a low endpoint should come before a high endpoint.
The extension method `IsValidInterval` on `IInterval<T>` will return true if the interval is valid.
All methods in C5 Intervals require that the used intervals are valid, but since code contracts do not allow invariants on interfaces,
the user is expected to do this check manually to avoid unnecessary overhead on methods.

None of the interval data structures support changes made to the intervals in them.
If you wish to change the interval of an object in a data structure, you have to remove it, change it, and then reinsert it.
The objects are stored based on their interval, and changing it will most likely break the invariant.