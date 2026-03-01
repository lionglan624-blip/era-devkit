"""
Pytest configuration for kojo-mapper tests.

Adds parent directory to sys.path to allow importing verify scripts.
"""

import sys
from pathlib import Path

# Add kojo-mapper directory to path to allow importing verify_*.py modules
kojo_mapper_dir = Path(__file__).parent.parent
sys.path.insert(0, str(kojo_mapper_dir))
