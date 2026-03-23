"""
utils/config.py

Config loading utilities.
Merges a base default.yaml with an optional user-supplied override file.
"""

from __future__ import annotations

import copy
from pathlib import Path
from typing import Any, Dict

import yaml

_DEFAULT_CONFIG = Path(__file__).parent.parent.parent.parent / "config" / "default.yaml"


def _deep_merge(base: Dict, override: Dict) -> Dict:
    """Recursively merge *override* into *base* (override wins on conflicts)."""
    result = copy.deepcopy(base)
    for key, value in override.items():
        if key in result and isinstance(result[key], dict) and isinstance(value, dict):
            result[key] = _deep_merge(result[key], value)
        else:
            result[key] = value
    return result


def load_config(path: Optional[Path] = None) -> Dict[str, Any]:
    """
    Load configuration.

    Args:
        path: Optional path to a user config .yaml file.
              If provided, it is deep-merged over the defaults.

    Returns:
        Merged configuration dictionary.
    """
    from typing import Optional  # local to avoid circular at module level

    with open(_DEFAULT_CONFIG) as f:
        config = yaml.safe_load(f)

    if path:
        path = Path(path)
        if not path.exists():
            raise FileNotFoundError(f"Config file not found: {path}")
        with open(path) as f:
            user_config = yaml.safe_load(f) or {}
        config = _deep_merge(config, user_config)

    return config
