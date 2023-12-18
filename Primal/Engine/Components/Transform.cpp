#include "Transform.h"
#include "Entity.h"

namespace aur::transform
{

	namespace
	{
		utl::vector<math::v4> rotations;
		utl::vector<math::v3> positions;
		utl::vector<math::v3> scales;
	}

	transform_id aur::transform::create_transform(const init_info& info, game_entity::entity_id entity_id)
	{
		return transform_id();
	}

}


